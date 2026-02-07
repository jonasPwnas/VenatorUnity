using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Charge From Element Source")]
public class GoapActionLogic_ChargeFromSource : GoapActionLogic
{
    [Header("Settings")]
    [Tooltip("If true, will stop early if AtChargePoint becomes 0.")]
    [SerializeField] private bool m_failIfLeavePoint = true;

    public override bool CanRun(GoapAgent agent)
    {
        if (agent == null)
            return false;

        if (agent.WorldStateBuilder == null)
            return false;

        if (agent.GetComponent<GoapChargeTargetContext>() == null)
            return false;

        if (agent.GetComponent<IElementInventory>() == null)
            return false;

        return true;
    }

    public override void Begin(GoapAgent agent)
    {
        agent.ActionMemory.SetStart(this, Time.time, true);
        agent.ActionMemory.SetResult(this, true);

        // Start charging animation if you have a combat driver
        ICombatDriver combat = agent.GetComponent<ICombatDriver>();
        combat?.TryStartCharging();
    }

    public override bool Tick(GoapAgent agent, float dt)
    {
        if (agent == null || agent.WorldStateBuilder == null)
            return true;

        GoapWorldState ws = agent.WorldStateBuilder.WorldState;

        if (m_failIfLeavePoint && ws.GetInt(GoapStateKey.AtChargePoint, 0) == 0)
        {
            agent.ActionMemory.SetResult(this, false);
            return true;
        }

        GoapChargeTargetContext ctx = agent.GetComponent<GoapChargeTargetContext>();
        ParentElementSource src = ctx != null ? ctx.CurrentSource : null;

        if (src == null)
        {
            agent.ActionMemory.SetResult(this, false);
            return true;
        }

        float total = Mathf.Max(0.05f, src.ChargeElementTime());

        // Give charges in 3 steps (like your coroutine).
        // We compute how many "thirds" have passed and apply missing charges.
        if (!agent.ActionMemory.TryGetStart(this, out float startTime, out bool startedOk) || !startedOk)
        {
            agent.ActionMemory.SetResult(this, false);
            return true;
        }

        float elapsed = Time.time - startTime;

        int stepsTotal = 3;
        int stepsDone = Mathf.Clamp(Mathf.FloorToInt((elapsed / total) * stepsTotal), 0, stepsTotal);

        // Store last applied step using a component (simple, robust)
        GoapChargeStepMemory stepMem = agent.GetComponent<GoapChargeStepMemory>();
        if (stepMem == null)
            stepMem = agent.gameObject.AddComponent<GoapChargeStepMemory>();

        int alreadyApplied = stepMem.GetApplied(this);

        if (stepsDone > alreadyApplied)
        {
            int toApply = stepsDone - alreadyApplied;

            IElementInventory inv = agent.GetComponent<IElementInventory>();
            ElementTypes.ElementType e = src.GetSourceElement();

            // Clamp to max 3 charges
            int current = inv.GetCharges(e);
            int add = Mathf.Min(toApply, 3 - current);

            if (add > 0)
                inv.AddCharges(e, add);

            stepMem.SetApplied(this, stepsDone);
        }

        if (elapsed >= total)
        {
            agent.ActionMemory.SetResult(this, true);
            return true;
        }

        return false;
    }

    public override bool HasFinishedSuccessfully(GoapAgent agent)
    {
        if (agent != null && agent.ActionMemory.TryGetResult(this, out bool has, out bool success) && has)
            return success;

        return true;
    }

    public override void End(GoapAgent agent, bool success)
    {
        if (agent == null)
            return;

        // Stop charging animation
        ICombatDriver combat = agent.GetComponent<ICombatDriver>();
        combat?.StopCharging();

        // Clear step memory for this action
        GoapChargeStepMemory stepMem = agent.GetComponent<GoapChargeStepMemory>();
        stepMem?.Clear(this);

        agent.ActionMemory.Clear(this);
    }
}
