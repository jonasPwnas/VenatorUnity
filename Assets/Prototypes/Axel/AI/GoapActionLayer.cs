using System.Collections;
using Characters;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapActionLayer : MonoBehaviour, IGoapActionListener
{
    public delegate void AnimEventFinished(bool lookDone, bool blockDone);
    public static event AnimEventFinished OnAnimEventFinished;

    private const string SIGNAL_FACE_DONE = "FaceDone";
    private const string SIGNAL_MOVE_DONE = "MoveDone";
    private const string SIGNAL_WANDER_DONE = "WanderDone";
    private const string SIGNAL_LOOK_DONE = "LookDone";
    private const string SIGNAL_DONE = "Done";

    [Header("References")]
    [SerializeField] private GoapAgent m_agent;
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private NavMeshAgent m_navMeshAgent;
    [SerializeField] private GoapAttackCooldownDriver m_attackCooldownDriver;
    [SerializeField] private Transform m_player;

    [Header("Patrol")]
    [SerializeField] private Transform[] m_patrolPoints;
    [SerializeField] private float m_arriveDistance = 0.6f;

    [Header("Facing")]
    [SerializeField] private float m_faceDegreesPerSecond = 720f;

    [Header("Tactical Movement")]
    [SerializeField] private float m_strafeDistance = 2.0f;
    [SerializeField] private float m_backAwayDistance = 2.0f;

    [Header("Maintain Distance (by CombatMode)")]
    [SerializeField] private float m_aggressiveDistance = 1.9f;
    [SerializeField] private float m_tacticalDistance = 2.8f;
    [SerializeField] private float m_baitingDistance = 3.6f;
    [SerializeField] private float m_elementalDistance = 5.0f;
    [SerializeField] private float m_distanceTolerance = 0.35f;

    [Header("Search")]
    [SerializeField] private float m_searchRadius = 10f;

    [Header("Animation Events")]
    [SerializeField] private bool m_useAnimationEvents = true;

    [Header("Combo")]
    [SerializeField] private int m_comboMinPressure = 2;
    [SerializeField] private int m_comboMaxPressure = 3;
    [SerializeField] private int m_comboMinReset = 1;
    [SerializeField] private int m_comboMaxReset = 2;

    private int m_currentPatrolIndex;
    private bool m_isChasing;
    private string m_currentActionId;
    private Coroutine m_runningRoutine;

    private bool m_pendingLightCooldown;
    private bool m_pendingHeavyCooldown;
    private bool m_pendingCastCooldown;

    private EnemyKnightRootMotionController m_motor;
    private ICombatDriver m_combat;
    private IElementInventory m_elements;

    // combo runtime
    private bool m_comboActive;
    private int m_comboPlannedHits;
    private int m_comboCompletedHits;
    private int m_comboQueuedHits; // counts triggers sent (includes first)

    private void Awake()
    {
        AutoGetReferencesIfMissing();

        if (m_motor != null)
        {
            m_motor.ComboWindowStarted += OnComboWindowStarted;
            m_motor.ComboWindowEnded += OnComboWindowEnded;
        }
    }

    private void OnDestroy()
    {
        if (m_motor != null)
        {
            m_motor.ComboWindowStarted -= OnComboWindowStarted;
            m_motor.ComboWindowEnded -= OnComboWindowEnded;
        }
    }

    private void Update()
    {
        if (m_isChasing)
            TickChaseTarget();
    }

    public void OnGoapActionEvent(GoapAgent agent, string actionId, GoapActionPhase phase)
    {
        if (phase == GoapActionPhase.Started)
        {
            StartAction(actionId);
            return;
        }

        if (phase == GoapActionPhase.Completed || phase == GoapActionPhase.Failed || phase == GoapActionPhase.Aborted)
        {
            StopAction(actionId);
            return;
        }
    }

    public void AnimEvent_Done()
    {
        if (m_agent == null)
            return;

        if (m_comboActive && NormalizeActionId(m_currentActionId) == "LightCombo")
        {
            m_comboCompletedHits++;

            if (m_comboCompletedHits < m_comboPlannedHits)
            {
                return;
            }
        }

        if (m_attackCooldownDriver != null)
        {
            if (m_pendingLightCooldown)
                m_attackCooldownDriver.StartLightAttackCooldown();
            if (m_pendingHeavyCooldown)
                m_attackCooldownDriver.StartHeavyAttackCooldown();
            if (m_pendingCastCooldown)
                m_attackCooldownDriver.StartCastCooldown();
        }

        m_pendingLightCooldown = false;
        m_pendingHeavyCooldown = false;
        m_pendingCastCooldown = false;

        m_comboActive = false;

        m_agent.SetSignal(SIGNAL_DONE);
        OnAnimEventFinished?.Invoke(false, true);
    }

    public void AnimEvent_LookDone()
    {
        if (m_agent == null)
            return;

        m_agent.SetSignal(SIGNAL_LOOK_DONE);
        OnAnimEventFinished?.Invoke(true, false);
    }

    private void OnComboWindowStarted()
    {
        if (!m_comboActive)
            return;

        if (m_comboQueuedHits >= m_comboPlannedHits)
            return;

        if (m_motor != null)
        {
            m_comboQueuedHits++;
            m_motor.DoLightAttack();
        }
    }

    private void OnComboWindowEnded()
    {
        // no-op (but left here if you want to debug or force behavior later)
    }

    private void StartAction(string actionId)
    {
        m_currentActionId = actionId;

        StopCurrentRoutine();
        m_isChasing = false;

        string id = NormalizeActionId(actionId);

        switch (id)
        {
            case "FaceTarget":
                m_runningRoutine = StartCoroutine(RunFaceTarget());
                break;

            case "ChaseTarget":
                StartChaseTarget();
                break;

            case "MoveToNextPatrolPoint":
                m_runningRoutine = StartCoroutine(RunMoveToNextPatrolPoint());
                break;

            case "MoveToLastKnownPosition":
                m_runningRoutine = StartCoroutine(RunMoveToLastKnownPosition());
                break;

            case "SearchAreaSweep":
                m_runningRoutine = StartCoroutine(RunSearchAreaSweep());
                break;

            case "SearchLookAround":
            case "ScanForPlayer":
                m_runningRoutine = StartCoroutine(RunLookAround(SIGNAL_LOOK_DONE));
                break;

            case "EnterBlock":
                m_runningRoutine = StartCoroutine(RunSetBlocking(true));
                break;

            case "ExitBlock":
                m_runningRoutine = StartCoroutine(RunSetBlocking(false));
                break;

            case "RollDodge":
                m_runningRoutine = StartCoroutine(RunRoll());
                break;

            case "LightCombo":
                m_runningRoutine = StartCoroutine(RunLightCombo());
                break;

            case "HeavyAttack":
                m_runningRoutine = StartCoroutine(RunHeavyAttack());
                break;

            case "CastFireAoE":
                m_runningRoutine = StartCoroutine(RunCast(ElementTypes.ElementType.Fire, cost: 2, CastType.Fire));
                break;

            case "CastWaterHeal":
                m_runningRoutine = StartCoroutine(RunCast(ElementTypes.ElementType.Water, cost: 1, CastType.Water));
                break;

            case "CastWindPush":
                m_runningRoutine = StartCoroutine(RunCast(ElementTypes.ElementType.Air, cost: 1, CastType.Wind));
                break;

            case "StrafeLeft":
                m_runningRoutine = StartCoroutine(RunStrafe(-1f));
                break;

            case "StrafeRight":
                m_runningRoutine = StartCoroutine(RunStrafe(1f));
                break;

            case "BackAway":
                m_runningRoutine = StartCoroutine(RunBackAway());
                break;

            case "MaintainCombatDistance":
                m_runningRoutine = StartCoroutine(RunMaintainCombatDistance());
                break;

            case "WaitAtPatrolPoint":
            case "GiveUpSearch":
                StopMovement();
                break;
        }
    }

    private void StopAction(string actionId)
    {
        string id = NormalizeActionId(actionId);

        if (id == "ChaseTarget")
            m_isChasing = false;

        StopCurrentRoutine();
        m_currentActionId = null;

        m_comboActive = false;
        m_comboPlannedHits = 0;
        m_comboCompletedHits = 0;
        m_comboQueuedHits = 0;
    }

    private void StopCurrentRoutine()
    {
        if (m_runningRoutine == null)
            return;

        StopCoroutine(m_runningRoutine);
        m_runningRoutine = null;
    }

    private string NormalizeActionId(string actionId)
    {
        if (string.IsNullOrEmpty(actionId))
            return actionId;

        // keep your old aliases
        if (actionId == "InvestigateLastKnow" || actionId == "MoveToLastKnowPositin" || actionId == "MoveToLastKnowPosition")
            return "MoveToLastKnownPosition";

        if (actionId == "SearachSweep" || actionId == "SearchSweep" || actionId == "SearchAreaWander")
            return "SearchAreaSweep";

        if (actionId == "PatrolScan")
            return "ScanForPlayer";

        if (actionId == "WaitAtPatrolePoint" || actionId == "WaitAtPatrolPoint")
            return "WaitAtPatrolPoint";

        if (actionId == "GiveUpSearchReturnToPatrole" || actionId == "GiveUpSearchReturnToPatrol" || actionId == "GiveUpSearch")
            return "GiveUpSearch";

        // new aliases (if you typo in assets)
        if (actionId == "LightAttackCombo" || actionId == "LightAttack")
            return "LightCombo";

        if (actionId == "StrongAttack" || actionId == "Heavy")
            return "HeavyAttack";

        return actionId;
    }

    private IEnumerator RunFaceTarget()
    {
        if (m_agent == null)
            yield break;

        Transform target = GetTargetTransform();
        if (target == null)
        {
            m_agent.SetSignal(SIGNAL_FACE_DONE);
            yield break;
        }

        while (true)
        {
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
                break;

            Quaternion desired = Quaternion.LookRotation(toTarget.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, m_faceDegreesPerSecond * Time.deltaTime);

            float dot = Vector3.Dot(transform.forward, toTarget.normalized);
            if (dot >= 0.985f)
                break;

            yield return null;
        }

        m_agent.SetSignal(SIGNAL_FACE_DONE);
    }

    private void StartChaseTarget()
    {
        m_isChasing = true;

        if (m_navMeshAgent != null)
            m_navMeshAgent.isStopped = false;
    }

    private void TickChaseTarget()
    {
        if (m_agent == null || m_navMeshAgent == null)
            return;

        Transform target = GetTargetTransform();
        if (target == null)
            return;

        m_navMeshAgent.SetDestination(target.position);

        // Chase ends when in melee (sensor sets TargetRange)
        if (m_worldStateBuilder == null)
            return;

        int range = m_worldStateBuilder.WorldState.GetInt(GoapStateKey.TargetRange, 0);
        if (range >= 3)
        {
            m_isChasing = false;
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
        }
    }

    private IEnumerator RunMoveToNextPatrolPoint()
    {
        if (m_agent == null)
            yield break;

        if (m_patrolPoints == null || m_patrolPoints.Length == 0)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        m_currentPatrolIndex++;
        if (m_currentPatrolIndex >= m_patrolPoints.Length)
            m_currentPatrolIndex = 0;

        Vector3 destination = m_patrolPoints[m_currentPatrolIndex].position;

        yield return RunMoveTo(destination);
        m_agent.SetSignal(SIGNAL_MOVE_DONE);
    }

    private IEnumerator RunMoveToLastKnownPosition()
    {
        if (m_agent == null)
            yield break;

        Vector3 destination = GetLastKnownPositionFallback();
        yield return RunMoveTo(destination);
        m_agent.SetSignal(SIGNAL_MOVE_DONE);
    }

    private IEnumerator RunSearchAreaSweep()
    {
        if (m_agent == null)
            yield break;

        Vector3 destination = GetRandomAreaAroundTargetFallback();
        yield return RunMoveTo(destination);
        m_agent.SetSignal(SIGNAL_WANDER_DONE);
    }

    private IEnumerator RunLookAround(string signalId)
    {
        if (m_agent == null)
            yield break;

        if (m_motor != null)
            m_motor.DoLookAround();

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(1.0f);
        m_agent.SetSignal(signalId);
    }

    private IEnumerator RunSetBlocking(bool isBlocking)
    {
        if (m_agent == null)
            yield break;

        StopMovement();

        if (m_motor != null)
            m_motor.SetBlocking(isBlocking);

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(0.25f);
        m_agent.SetSignal(SIGNAL_DONE);
    }

    private IEnumerator RunRoll()
    {
        if (m_agent == null)
            yield break;

        StopMovement();

        if (m_motor != null)
        {
            m_pendingCastCooldown = false;
            m_pendingHeavyCooldown = false;
            m_pendingLightCooldown = false;

            m_motor.DoRoll();
        }

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(0.7f);
        m_agent.SetSignal(SIGNAL_DONE);
    }

    private IEnumerator RunLightCombo()
    {
        if (m_agent == null)
            yield break;

        StopMovement();

        m_comboActive = true;
        m_comboCompletedHits = 0;
        m_comboQueuedHits = 1;

        int pressure = (m_worldStateBuilder != null) ? m_worldStateBuilder.WorldState.GetInt(GoapStateKey.PressureState, 0) : 0;

        int min = pressure == 1 ? m_comboMinPressure : m_comboMinReset;
        int max = pressure == 1 ? m_comboMaxPressure : m_comboMaxReset;

        m_comboPlannedHits = Random.Range(min, max + 1);

        m_pendingLightCooldown = true;
        m_pendingHeavyCooldown = false;
        m_pendingCastCooldown = false;

        if (m_motor != null)
            m_motor.DoLightAttack();

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(0.65f);
        m_agent.SetSignal(SIGNAL_DONE);
    }

    private IEnumerator RunHeavyAttack()
    {
        if (m_agent == null)
            yield break;

        StopMovement();

        m_pendingHeavyCooldown = true;
        m_pendingLightCooldown = false;
        m_pendingCastCooldown = false;

        if (m_motor != null)
            m_motor.DoStrongAttack();

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(1.1f);
        m_agent.SetSignal(SIGNAL_DONE);
    }

    private enum CastType { Fire, Water, Wind }

    private IEnumerator RunCast(ElementTypes.ElementType element, int cost, CastType type)
    {
        if (m_agent == null)
            yield break;

        StopMovement();

        if (m_elements != null)
        {
            if (!m_elements.TrySpend(element, cost))
            {
                m_agent.SetSignal(SIGNAL_DONE);
                yield break;
            }
        }

        m_pendingCastCooldown = true;
        m_pendingLightCooldown = false;
        m_pendingHeavyCooldown = false;

        if (m_motor != null)
        {
            if (type == CastType.Fire) m_motor.DoFireAoE();
            if (type == CastType.Water) m_motor.DoWaterHeal();
            if (type == CastType.Wind) m_motor.DoWindPush();
        }

        if (m_useAnimationEvents)
            yield break;

        yield return new WaitForSeconds(1.2f);
        m_agent.SetSignal(SIGNAL_DONE);
    }

    private IEnumerator RunStrafe(float sign)
    {
        if (m_agent == null)
            yield break;

        Transform target = GetTargetTransform();
        if (target == null)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 dir = toTarget.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;

        Vector3 desired = transform.position + right * (m_strafeDistance * sign);

        desired = SampleNav(desired);

        yield return RunMoveTo(desired);
        m_agent.SetSignal(SIGNAL_MOVE_DONE);
    }

    private IEnumerator RunBackAway()
    {
        if (m_agent == null)
            yield break;

        Transform target = GetTargetTransform();
        if (target == null)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 away = (transform.position - target.position);
        away.y = 0f;
        if (away.sqrMagnitude < 0.001f)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 dest = transform.position + away.normalized * m_backAwayDistance;
        dest = SampleNav(dest);

        yield return RunMoveTo(dest);
        m_agent.SetSignal(SIGNAL_MOVE_DONE);
    }

    private IEnumerator RunMaintainCombatDistance()
    {
        if (m_agent == null)
            yield break;

        Transform target = GetTargetTransform();
        if (target == null)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        float desired = GetDesiredDistanceByMode();

        float d = Vector3.Distance(transform.position, target.position);
        if (Mathf.Abs(d - desired) <= m_distanceTolerance)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 dirToTarget = (target.position - transform.position);
        dirToTarget.y = 0f;
        if (dirToTarget.sqrMagnitude < 0.001f)
        {
            m_agent.SetSignal(SIGNAL_MOVE_DONE);
            yield break;
        }

        Vector3 moveDir;

        if (d < desired)
            moveDir = -dirToTarget.normalized; // move away
        else
            moveDir = dirToTarget.normalized;  // move closer

        float step = Mathf.Clamp(Mathf.Abs(d - desired), 0.6f, 2.5f);
        Vector3 dest = transform.position + moveDir * step;
        dest = SampleNav(dest);

        yield return RunMoveTo(dest);
        m_agent.SetSignal(SIGNAL_MOVE_DONE);
    }

    private float GetDesiredDistanceByMode()
    {
        if (m_worldStateBuilder == null)
            return m_tacticalDistance;

        int mode = m_worldStateBuilder.WorldState.GetInt(GoapStateKey.CombatMode, 0);

        switch ((GoapCombatMode)mode)
        {
            case GoapCombatMode.Aggressive: return m_aggressiveDistance;
            case GoapCombatMode.Baiting: return m_baitingDistance;
            case GoapCombatMode.Elemental: return m_elementalDistance;
            case GoapCombatMode.Tactical: return m_tacticalDistance;
            default: return m_tacticalDistance;
        }
    }

    private IEnumerator RunMoveTo(Vector3 destination)
    {
        if (m_navMeshAgent == null)
            yield break;

        if (!m_navMeshAgent.isOnNavMesh)
            yield break;

        m_navMeshAgent.isStopped = false;
        m_navMeshAgent.SetDestination(destination);

        while (!HasArrivedAtDestination())
            yield return null;
    }

    private bool HasArrivedAtDestination()
    {
        if (m_navMeshAgent == null)
            return true;

        if (!m_navMeshAgent.isOnNavMesh)
            return true;

        if (m_navMeshAgent.pathPending)
            return false;

        float stopping = Mathf.Max(m_navMeshAgent.stoppingDistance, m_arriveDistance);

        if (m_navMeshAgent.remainingDistance > stopping)
            return false;

        return m_navMeshAgent.velocity.sqrMagnitude < 0.02f;
    }

    private void StopMovement()
    {
        if (m_navMeshAgent == null)
            return;

        m_navMeshAgent.isStopped = true;
    }

    private Transform GetTargetTransform()
    {
        if (m_worldStateBuilder != null && m_worldStateBuilder.TargetTransform != null)
            return m_worldStateBuilder.TargetTransform;

        return m_player;
    }

    private Vector3 GetLastKnownPositionFallback()
    {
        if (m_worldStateBuilder != null)
            return m_worldStateBuilder.LastKnownTargetPosition;

        Transform target = GetTargetTransform();
        if (target != null)
            return target.position;

        return transform.position;
    }

    private Vector3 GetRandomAreaAroundTargetFallback()
    {
        Vector3 center = GetLastKnownPositionFallback();

        Vector3 randomDirection = Random.insideUnitSphere * m_searchRadius;
        randomDirection += center;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, m_searchRadius, -1);
        return navHit.position;
    }

    private Vector3 SampleNav(Vector3 pos)
    {
        if (m_navMeshAgent == null)
            return pos;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 2.5f, -1))
            return hit.position;

        return pos;
    }

    private void AutoGetReferencesIfMissing()
    {
        m_motor = GetComponent<EnemyKnightRootMotionController>();
        m_combat = GetComponent<ICombatDriver>();
        m_elements = GetComponent<IElementInventory>();

        if (m_agent == null)
            m_agent = GetComponent<GoapAgent>();

        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_navMeshAgent == null)
            m_navMeshAgent = GetComponent<NavMeshAgent>();

        if (m_attackCooldownDriver == null)
            m_attackCooldownDriver = GetComponent<GoapAttackCooldownDriver>();

        if (m_player == null)
            m_player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_searchRadius);
    }
}
