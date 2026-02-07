using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Logic/Search Area Wander (Event Driven)")]
public class GoapActionLogic_SearchAreaWander : GoapActionLogic
{
    [Tooltip("If > 0, auto-completes after this duration. If 0, requires a signal.")]
    [SerializeField] private float m_durationSeconds = 3f;

    [Tooltip("External wander/movement system can complete early by calling agent.SetSignal(...) with this id.")]
    [SerializeField] private string m_completeSignalId = "WanderDone";

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

        if (m_durationSeconds <= 0f)
            return false;

        int id = agent.GetInstanceID();
        float startTime = m_startTimes.TryGetValue(id, out float t) ? t : Time.time;

        return (Time.time - startTime) >= m_durationSeconds;
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