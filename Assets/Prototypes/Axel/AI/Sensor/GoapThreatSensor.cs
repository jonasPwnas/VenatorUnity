using UnityEngine;

[DisallowMultipleComponent]
public class GoapThreatSensor : MonoBehaviour, IGoapSensor
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Threat tuning")]
    [SerializeField] private int m_baseThreatIfAttacking = 80;
    [SerializeField] private int m_bonusIfVeryClose = 15;
    [SerializeField] private float m_veryCloseDistance = 1.5f;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (m_worldStateBuilder == null)
            return;

        var ws = m_worldStateBuilder.WorldState;

        bool visible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);
        bool targetAttacking = ws.GetInt(GoapStateKey.TargetAttackingNow, 0) == 1;

        int threat = 0;

        if (visible && range == 3 && targetAttacking)
        {
            threat = m_baseThreatIfAttacking;

            Transform t = m_worldStateBuilder.TargetTransform;
            if (t != null)
            {
                float d = Vector3.Distance(transform.position, t.position);
                if (d <= m_veryCloseDistance)
                    threat = Mathf.Clamp(threat + m_bonusIfVeryClose, 0, 100);
            }
        }

        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.IncomingThreat01, threat);
    }
}
