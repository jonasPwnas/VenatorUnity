using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapCombatLocomotionController : MonoBehaviour
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private NavMeshAgent m_nav;
    [SerializeField] private float m_faceDegreesPerSecond = 720f;

    [Header("Lock-on rules")]
    [SerializeField] private bool m_lockOnWhenTargetKnown = true;
    [SerializeField] private int m_lockOnMinRange = 2; // 2=chase, 3=melee

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
        if (m_nav == null)
            m_nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (m_worldStateBuilder == null || m_nav == null)
            return;

        var ws = m_worldStateBuilder.WorldState;

        bool targetKnown = ws.GetInt(GoapStateKey.TargetKnown, 0) == 1;
        bool targetVisible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);

        bool wantLockOn = (targetVisible || (m_lockOnWhenTargetKnown && targetKnown)) && range >= m_lockOnMinRange;

        ws.SetInt(GoapStateKey.IsLockedOn, wantLockOn ? 1 : 0);

        if (!wantLockOn)
        {
            m_nav.updateRotation = true;
            return;
        }

        m_nav.updateRotation = false;

        Transform target = m_worldStateBuilder.TargetTransform;
        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, m_faceDegreesPerSecond * Time.deltaTime);
    }
}
