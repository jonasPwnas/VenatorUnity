using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Charge Element")]
public class GoapActionLogic_ChargeElement : GoapActionLogic
{
    [SerializeField] private float m_chargeSeconds = 2.0f;
    [SerializeField] private ElementTypes.ElementType m_elementToCharge = ElementTypes.ElementType.Fire;
    [SerializeField] private int m_addCharges = 1;

    public override bool CanRun(GoapAgent agent)
    {
        return agent != null && agent.WorldStateBuilder != null && agent.GetComponent<ICombatDriver>() != null && agent.GetComponent<IElementInventory>() != null;
    }

    public override void Begin(GoapAgent agent)
    {
        bool atPoint = agent.WorldStateBuilder.WorldState.GetInt(GoapStateKey.AtChargePoint, 0) == 1;
        bool ok = atPoint;

        agent.ActionMemory.SetStart(this, Time.time, ok);

        if (!ok)
        {
            agent.ActionMemory.SetResult(this, false);
            return;
        }

        ICombatDriver combat = agent.GetComponent<ICombatDriver>();
        bool started = combat.TryStartCharging();
        agent.ActionMemory.SetResult(this, started);
    }

    public override bool Tick(GoapAgent agent, float dt)
    {
        if (agent == null)
            return true;

        if (!agent.ActionMemory.TryGetStart(this, out float startTime, out bool startedOk) || !startedOk)
            return true;

        if ((Time.time - startTime) < m_chargeSeconds)
            return false;

        IElementInventory inv = agent.GetComponent<IElementInventory>();
        if (inv != null)
            inv.AddCharges(m_elementToCharge, m_addCharges);

        return true;
    }

    public override bool HasFinishedSuccessfully(GoapAgent agent)
    {
        if (agent != null && agent.ActionMemory.TryGetResult(this, out bool has, out bool success) && has)
            return success;

        return true;
    }

    public override void End(GoapAgent agent, bool success)
    {
        if (agent != null)
        {
            ICombatDriver combat = agent.GetComponent<ICombatDriver>();
            combat?.StopCharging();

            agent.ActionMemory.Clear(this);
        }
    }
}
