using UnityEngine;

[DisallowMultipleComponent]
public class GoapTouchSensor : MonoBehaviour, IGoapSensor
{
    [Tooltip("If empty, auto-finds GoapWorldStateBuilder on this GameObject.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Target")]
    [Tooltip("If empty, will try to read target from GoapWorldStateBuilder (TargetTransform).")]
    [SerializeField] private Transform m_target;

    [Tooltip("If true, touching will also set TargetTransform if missing.")]
    [SerializeField] private bool m_setTargetTransformOnTouch = true;

    [Header("Confidence")]
    [SerializeField] private float m_confidence = 1f;

    [Header("Touch Memory")]
    [Tooltip("How long after losing touch we still consider the target 'known'.")]
    [SerializeField] private float m_touchMemorySeconds = 0.25f;


    [Header("Touching")]
    [Tooltip("Distance to consider 'touching'.")]
    [SerializeField] private float m_touchingDistance = 1.0f;

    [Tooltip("Offset of the touch box from the GameObject's position.")]
    [SerializeField] private Vector3 m_touchBoxOffset = Vector3.zero;

    private bool m_isTouchingTarget;
    private float m_lastTouchTime;
    private Transform m_lastTouchedTarget;

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

        if (!m_isTouchingTarget && Time.time - m_lastTouchTime > m_touchMemorySeconds)
            return;

        if (m_lastTouchedTarget == null)
            return;

        Vector3 targetPos = m_lastTouchedTarget.position;
        float distance = Vector3.Distance(transform.position + m_touchBoxOffset, targetPos);

        blackboard.ReportVisibleTarget(m_confidence, targetPos, distance);

        if (m_setTargetTransformOnTouch && m_worldStateBuilder != null && m_worldStateBuilder.TargetTransform == null)
            m_worldStateBuilder.SetTargetTransform(m_lastTouchedTarget);
    }

    private void Update()
    {
        if (m_target == null)
            return;
        float distance = Vector3.Distance(transform.position + m_touchBoxOffset , m_target.position);
        bool isTouchingNow = distance <= m_touchingDistance;
        if (isTouchingNow && !m_isTouchingTarget)
        {
            m_isTouchingTarget = true;
            m_lastTouchTime = Time.time;
            m_lastTouchedTarget = m_target;
        }
        else if (!isTouchingNow && m_isTouchingTarget)
        {
            m_isTouchingTarget = false;
            m_lastTouchTime = Time.time;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!IsTarget(other))
    //        return;

    //    m_isTouchingTarget = true;
    //    m_lastTouchTime = Time.time;
    //    m_lastTouchedTarget = other.transform;
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!IsTarget(other))
    //        return;

    //    m_isTouchingTarget = false;
    //    m_lastTouchTime = Time.time;
    //}

    //private bool IsTarget(Collider other)
    //{
    //    if (other == null)
    //        return false;

    //    if (other.transform == m_target)
    //        return true;

    //    if (m_worldStateBuilder != null && m_worldStateBuilder.TargetTransform != null)
    //        return other.transform == m_worldStateBuilder.TargetTransform;

    //    return false;
    //}

    private void AutoGetReferencesIfMissing()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        //if (m_collider == null)
        //    m_collider = GetComponent<Collider>();

        if (m_target == null)
            m_target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + m_touchBoxOffset, Vector3.one * m_touchingDistance);
    }
}
