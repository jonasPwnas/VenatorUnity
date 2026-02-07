using UnityEngine;

[DisallowMultipleComponent]
public class GoapFacingSensor : MonoBehaviour, IGoapSensor
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private float m_facingDotThreshold = 0.85f;

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
        {
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.FacingTarget, 0);
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
        {
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.FacingTarget, 1);
            return;
        }

        float dot = Vector3.Dot(transform.forward, toTarget.normalized);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.FacingTarget, dot >= m_facingDotThreshold ? 1 : 0);
    }
}
