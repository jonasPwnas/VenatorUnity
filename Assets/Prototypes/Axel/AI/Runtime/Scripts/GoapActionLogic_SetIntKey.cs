using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Set Int Key")]
public class GoapActionLogic_SetIntKey : GoapActionLogic
{
    [SerializeField] private GoapStateKey m_key;
    [SerializeField] private int m_value;

    public override bool CanRun(GoapAgent agent) => agent != null && agent.WorldStateBuilder != null;

    public override void Begin(GoapAgent agent)
    {
        agent.WorldStateBuilder.SetInt(m_key, m_value);
        agent.ActionMemory.SetResult(this, true);
    }

    public override bool Tick(GoapAgent agent, float dt) => true;

    public override bool HasFinishedSuccessfully(GoapAgent agent) => true;

    public override void End(GoapAgent agent, bool success)
    {
        if (agent != null)
            agent.ActionMemory.Clear(this);
    }
}
