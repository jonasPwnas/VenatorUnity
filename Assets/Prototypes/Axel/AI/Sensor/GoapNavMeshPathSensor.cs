using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapNavMeshPathSensor : MonoBehaviour, IGoapSensor
{
    [Header("References")]
    [Tooltip("If empty, auto-finds NavMeshAgent on this GameObject.")]
    [SerializeField] private NavMeshAgent m_navMeshAgent;

    [Tooltip("If empty, will try to read target from GoapWorldStateBuilder (TargetTransform).")]
    [SerializeField] private Transform m_target;

    [Tooltip("If true, uses LastKnownTargetPosition when target transform is missing.")]
    [SerializeField] private bool m_fallbackToLastKnownPosition = true;

    [Tooltip("If empty, auto-finds GoapWorldStateBuilder on this GameObject.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Path Settings")]
    [Tooltip("How often to recompute path checks (seconds).")]
    [SerializeField] private float m_repathInterval = 0.2f;

    [Tooltip("Confidence used when reporting path info.")]
    [SerializeField] private float m_confidence = 1f;

    private float m_nextRepathTime;
    private bool m_cachedHasPath;

    private NavMeshPath m_path;

    private void Awake()
    {
        AutoGetReferencesIfMissing();
        m_path = new NavMeshPath();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (blackboard == null)
            return;

        if (m_navMeshAgent == null)
            return;

        Vector3 destination;
        if (!TryGetDestination(out destination))
            return;

        if (Time.time < m_nextRepathTime)
        {
            blackboard.ReportHasPathToTarget(m_confidence, m_cachedHasPath);
            return;
        }

        m_nextRepathTime = Time.time + Mathf.Max(0.05f, m_repathInterval);

        bool hasPath = CalculateHasCompletePath(destination);
        m_cachedHasPath = hasPath;

        blackboard.ReportHasPathToTarget(m_confidence, hasPath);
    }

    private bool TryGetDestination(out Vector3 destination)
    {
        destination = Vector3.zero;

        Transform t = m_target;

        if (t == null && m_worldStateBuilder != null)
            t = m_worldStateBuilder.TargetTransform;

        if (t != null)
        {
            destination = t.position;
            return true;
        }

        if (m_fallbackToLastKnownPosition && m_worldStateBuilder != null)
        {
            destination = m_worldStateBuilder.LastKnownTargetPosition;

            if (destination == Vector3.zero)
                return false;

            return true;
        }

        return false;
    }

    private bool CalculateHasCompletePath(Vector3 destination)
    {
        if (!m_navMeshAgent.isOnNavMesh)
            return false;

        bool success = m_navMeshAgent.CalculatePath(destination, m_path);
        if (!success)
            return false;

        return m_path.status == NavMeshPathStatus.PathComplete;
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_navMeshAgent == null)
            m_navMeshAgent = GetComponent<NavMeshAgent>();

        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_target == null)
            m_target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
}



