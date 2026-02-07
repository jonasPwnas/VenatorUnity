using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Charge Air Anywhere")]
public class GoapActionLogic_ChargeAirAnywhere : GoapActionLogic
{
    [Tooltip("Total time to gain 3 air charges (mirrors your player logic default of ~6 seconds).")]
    [SerializeField] private float m_totalSeconds = 6f;

    public override bool CanRun(GoapAgent agent)
    {
        return agent != null && agent.GetComponent<IElementInventory>() != null;
    }

    public override void Begin(GoapAgent agent)
    {
        agent.ActionMemory.SetStart(this, Time.time, true);
        agent.ActionMemory.SetResult(this, true);

        ICombatDriver combat = agent.GetComponent<ICombatDriver>();
        combat?.TryStartCharging();
    }

    public override bool Tick(GoapAgent agent, float dt)
    {
        if (agent == null)
            return true;

        if (!agent.ActionMemory.TryGetStart(this, out float startTime, out bool startedOk) || !startedOk)
        {
            agent.ActionMemory.SetResult(this, false);
            return true;
        }

        float total = Mathf.Max(0.05f, m_totalSeconds);
        float elapsed = Time.time - startTime;

        int stepsTotal = 3;
        int stepsDone = Mathf.Clamp(Mathf.FloorToInt((elapsed / total) * stepsTotal), 0, stepsTotal);

        GoapChargeStepMemory stepMem = agent.GetComponent<GoapChargeStepMemory>();
        if (stepMem == null)
            stepMem = agent.gameObject.AddComponent<GoapChargeStepMemory>();

        int alreadyApplied = stepMem.GetApplied(this);

        if (stepsDone > alreadyApplied)
        {
            int toApply = stepsDone - alreadyApplied;

            IElementInventory inv = agent.GetComponent<IElementInventory>();

            int current = inv.GetCharges(ElementTypes.ElementType.Air);
            int add = Mathf.Min(toApply, 3 - current);

            if (add > 0)
                inv.AddCharges(ElementTypes.ElementType.Air, add);

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

        ICombatDriver combat = agent.GetComponent<ICombatDriver>();
        combat?.StopCharging();

        GoapChargeStepMemory stepMem = agent.GetComponent<GoapChargeStepMemory>();
        stepMem?.Clear(this);

        agent.ActionMemory.Clear(this);
    }
}
