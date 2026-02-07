using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Wait")]
public class GoapActionLogic_Wait : GoapActionLogic
{
    [Tooltip("How long to wait. If 0, waiting requires a signal.")]
    [SerializeField] private float m_waitSeconds = 1f;

    [Tooltip("Optional signal that can end the wait early. Leave empty to ignore signals.")]
    [SerializeField] private string m_completeSignalId = "";

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

        if (m_waitSeconds <= 0f)
            return false;

        int id = agent.GetInstanceID();
        float startTime = m_startTimes.TryGetValue(id, out float t) ? t : Time.time;

        return (Time.time - startTime) >= m_waitSeconds;
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