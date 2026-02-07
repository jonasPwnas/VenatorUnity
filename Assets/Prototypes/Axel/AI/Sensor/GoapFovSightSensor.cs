using UnityEngine;

[DisallowMultipleComponent]
public class GoapFovSightSensor : MonoBehaviour, IGoapSensor
{
    [Header("References")]
    [Tooltip("Eye transform used for FOV and ray origin.")]
    [SerializeField] private Transform m_eye;

    [Tooltip("Target to look for. If empty, will try GoapWorldStateBuilder.TargetTransform.")]
    [SerializeField] private Transform m_target;

    [Tooltip("If empty, auto-finds GoapWorldStateBuilder on this GameObject.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("FOV")]
    [SerializeField] private float m_maxDistance = 20f;

    [Tooltip("Full field of view angle in degrees (eg 120).")]
    [SerializeField, Range(0, 360)] private float m_fovAngle = 120f;

    [Header("Forward Override")]
    [Tooltip("Optional forward rotation offset applied to the eye's forward (fix rigs where forward axis is wrong).")]
    [SerializeField] private Vector3 m_forwardEulerOffset = Vector3.zero;

    [Header("Line of Sight")]
    [Tooltip("Layers that can block vision.")]
    [SerializeField] private LayerMask m_blockingLayers;

    [Tooltip("Offsets for eye and target (helps aim at chest/head instead of feet).")]
    [SerializeField] private Vector3 m_eyeOffset = new Vector3(0f, 0.0f, 0f);
    [SerializeField] private Vector3 m_targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Confidence")]
    [SerializeField] private float m_confidence = 1f;

    private void Awake()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (blackboard == null)
            return;

        Transform eye = m_eye != null ? m_eye : transform;
        Transform target = ResolveTarget();

        if (target == null)
            return;

        Vector3 eyePos = eye.position + m_eyeOffset;
        Vector3 targetPos = target.position + m_targetOffset;

        Vector3 toTarget = targetPos - eyePos;
        float distance = toTarget.magnitude;

        if (distance <= 0.0001f)
            return;

        if (distance > m_maxDistance)
            return;

        Vector3 dir = toTarget / distance;

        Vector3 sensorForward = GetSensorForward(eye);

        if (!IsWithinFov(sensorForward, dir))
            return;

        if (IsLineOfSightBlocked(eyePos, dir, distance))
            return;

        blackboard.ReportVisibleTarget(m_confidence, target.position, distance);
    }

    private Transform ResolveTarget()
    {
        if (m_target != null)
            return m_target;

        if (m_worldStateBuilder != null && m_worldStateBuilder.TargetTransform != null)
            return m_worldStateBuilder.TargetTransform;

        return null;
    }

    private bool IsWithinFov(Vector3 forward, Vector3 directionToTarget)
    {
        float halfAngle = Mathf.Clamp(m_fovAngle * 0.5f, 0f, 180f);
        float dot = Vector3.Dot(forward.normalized, directionToTarget.normalized);

        float cosThreshold = Mathf.Cos(halfAngle * Mathf.Deg2Rad);
        return dot >= cosThreshold;
    }

    private bool IsLineOfSightBlocked(Vector3 origin, Vector3 dir, float distance)
    {
        return Physics.Raycast(origin, dir, distance, m_blockingLayers, QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetSensorForward(Transform eye)
    {
        Quaternion offset = Quaternion.Euler(m_forwardEulerOffset);
        return offset * eye.forward;
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_target == null)
            m_target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Transform eye = m_eye != null ? m_eye : transform;
        Vector3 eyePos = eye.position + m_eyeOffset;
        Vector3 forward = GetSensorForward(eye);

        for (float angle = -m_fovAngle / 2f; angle <= m_fovAngle / 2f; angle += 5f)
        {
            Quaternion rot = Quaternion.AngleAxis(angle, eye.up);
            Vector3 dir = rot * forward;
            Gizmos.DrawLine(eyePos, eyePos + dir.normalized * m_maxDistance);
        }
    }
}
