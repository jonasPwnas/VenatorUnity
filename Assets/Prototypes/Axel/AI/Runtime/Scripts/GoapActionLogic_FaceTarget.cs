using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Face Target (Event Driven)")]
public class GoapActionLogic_FaceTarget : GoapActionLogic
{
    [Tooltip("External facing/rotation system should complete by calling agent.SetSignal(...) with this id.")]
    [SerializeField] private string m_completeSignalId = "FaceDone";

    [Tooltip("Optional safety timeout. 0 = never timeout (signal required).")]
    [SerializeField] private float m_timeoutSeconds = 0f;

    private readonly Dictionary<int, float> m_startTimes = new Dictionary<int, float>(64);

    public override bool CanRun(GoapAgent agent)
    {
        return agent != null;
    }

    public override void Begin(GoapAgent agent)
    {
        int id = agent.GetInstanceID();
        m_startTimes[id] = Time.time;
    }

    public override bool Tick(GoapAgent agent, float dt)
    {
        if (agent == null)
            return true;

        if (!string.IsNullOrEmpty(m_completeSignalId) && agent.ConsumeSignal(m_completeSignalId))
            return true;

        if (m_timeoutSeconds > 0f)
        {
            int id = agent.GetInstanceID();
            float startTime = m_startTimes.TryGetValue(id, out float t) ? t : Time.time;

            if (Time.time - startTime >= m_timeoutSeconds)
                return true;
        }

        return false;
    }

    public override void End(GoapAgent agent, bool success)
    {
        if (agent == null)
            return;

        int id = agent.GetInstanceID();
        if (m_startTimes.ContainsKey(id))
            m_startTimes.Remove(id);
    }
}