using UnityEngine;

[DisallowMultipleComponent]
public class GoapCheeseSensor : MonoBehaviour, IGoapSensor
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Cheese detection")]
    [SerializeField] private float m_stagnantSecondsToCount = 1.2f;
    [SerializeField] private float m_requiredApproachPerSecond = 0.4f;
    [SerializeField] private float m_scoreIncreasePerSecond = 18f;
    [SerializeField] private float m_scoreDecayPerSecond = 25f;

    private float m_lastTime = -1f;
    private float m_lastDistance = -1f;
    private float m_stagnantTime;
    private float m_score01;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (m_worldStateBuilder == null)
            return;

        Transform target = m_worldStateBuilder.TargetTransform;
        if (target == null)
            return;

        float now = Time.time;
        float dt = (m_lastTime < 0f) ? 0f : (now - m_lastTime);
        m_lastTime = now;

        var ws = m_worldStateBuilder.WorldState;
        bool visible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);
        bool hasPath = ws.GetInt(GoapStateKey.HasPathToTarget, 0) == 1;

        float dist = Vector3.Distance(transform.position, target.position);

        if (m_lastDistance < 0f)
            m_lastDistance = dist;

        float approach = (m_lastDistance - dist) / Mathf.Max(0.0001f, dt);

        bool isChaseBand = (range == 2);

        if (visible && isChaseBand && hasPath && dt > 0f)
        {
            if (approach < m_requiredApproachPerSecond)
                m_stagnantTime += dt;
            else
                m_stagnantTime = Mathf.Max(0f, m_stagnantTime - dt * 2f);

            if (m_stagnantTime >= m_stagnantSecondsToCount)
                m_score01 += m_scoreIncreasePerSecond * dt;
            else
                m_score01 -= (m_scoreDecayPerSecond * 0.5f) * dt;
        }
        else
        {
            m_stagnantTime = Mathf.Max(0f, m_stagnantTime - dt * 3f);
            m_score01 -= m_scoreDecayPerSecond * dt;
        }

        m_score01 = Mathf.Clamp(m_score01, 0f, 100f);
        m_lastDistance = dist;

        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.CheeseScore01, Mathf.RoundToInt(m_score01));
    }
}
