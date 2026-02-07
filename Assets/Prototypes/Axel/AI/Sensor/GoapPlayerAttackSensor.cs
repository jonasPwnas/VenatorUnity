using Characters;
using Player;
using UnityEngine;

public class GoapPlayerAttackSensor : MonoBehaviour, IGoapSensor
{
    [Header("References")]

    [Tooltip("Player GameObject to monitor for attacks.")]
    [SerializeField] private GameObject m_player;

    [Tooltip("If empty, auto-finds GoapWorldStateBuilder on this GameObject.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Tooltip("Confidence used when reporting attack info.")]
    [SerializeField] private float m_confidence = 1f;

    private bool m_playerAttacking = false;
    

    private void Awake()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnEnable()
    {
            PlayerController.OnAttacking += OnPlayerAttacking;
    }

    private void OnDisable()
    {
            PlayerController.OnAttacking -= OnPlayerAttacking;
    }


    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        //Debug.LogError("GoapPlayerAttackSensor: Collecting attack info.");
        if (blackboard == null)
            return;
        //Debug.LogError("GoapPlayerAttackSensor: WorldState TargetVisible: " + m_worldStateBuilder.WorldState.GetInt(GoapStateKey.TargetVisible));
        if (m_worldStateBuilder.WorldState.GetInt(GoapStateKey.TargetVisible) == 0)
            return;
        //Debug.LogError("GoapPlayerAttackSensor: WorldState FacingTarget: " + m_worldStateBuilder.WorldState.GetInt(GoapStateKey.FacingTarget));
        if (m_worldStateBuilder.WorldState.GetInt(GoapStateKey.FacingTarget) == 0)
            return;

        //Debug.LogError("GoapPlayerAttackSensor: Reporting player attacking: " + m_playerAttacking);
        blackboard.ReportTargetAttackingNow(m_confidence, m_playerAttacking);
        m_playerAttacking = false;
    }

    private void OnPlayerAttacking()
    {
        //Debug.LogError("GoapPlayerAttackSensor: Detected player attacking.");
        m_playerAttacking = true;
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if(m_player == null)
            m_player = GameObject.FindGameObjectWithTag("Player");
    }
}
