using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Move To Charge Point")]
public class GoapActionLogic_MoveToChargePoint : GoapActionLogic
{
    [SerializeField] private float m_arriveDistance = 1.1f;
    [SerializeField] private float m_timeoutSeconds = 10f;

    public override bool CanRun(GoapAgent agent)
    {
        return agent != null && agent.WorldStateBuilder != null && agent.NavMeshAgent != null;
    }

    public override void Begin(GoapAgent agent)
    {
        GoapWorldStateBuilder b = agent.WorldStateBuilder;
        NavMeshAgent nav = agent.NavMeshAgent;

        bool ok = b.HasChargePoint && nav.isOnNavMesh;

        agent.ActionMemory.SetStart(this, Time.time, ok);

        if (!ok)
        {
            agent.ActionMemory.SetResult(this, false);
            return;
        }

        nav.isStopped = false;
        nav.SetDestination(b.ChargePointPosition);
    }

    public override bool Tick(GoapAgent agent, float dt)
    {
        if (agent == null || agent.NavMeshAgent == null)
            return true;

        if (agent.ActionMemory.TryGetStart(this, out float startTime, out bool startedOk))
        {
            if (!startedOk)
                return true;

            if (m_timeoutSeconds > 0f && (Time.time - startTime) >= m_timeoutSeconds)
            {
                agent.ActionMemory.SetResult(this, false);
                return true;
            }
        }

        NavMeshAgent nav = agent.NavMeshAgent;

        if (nav.pathPending)
            return false;

        float stop = Mathf.Max(nav.stoppingDistance, m_arriveDistance);

        if (nav.remainingDistance > stop)
            return false;

        bool arrived = nav.velocity.sqrMagnitude < 0.02f;
        if (arrived)
            agent.ActionMemory.SetResult(this, true);

        return arrived;
    }

    public override bool HasFinishedSuccessfully(GoapAgent agent)
    {
        if (agent != null && agent.ActionMemory.TryGetResult(this, out bool has, out bool success) && has)
            return success;

        return true;
    }

    public override void End(GoapAgent agent, bool success)
    {
        if (agent != null && agent.NavMeshAgent != null)
            agent.NavMeshAgent.isStopped = true;

        if (agent != null)
            agent.ActionMemory.Clear(this);
    }
}
