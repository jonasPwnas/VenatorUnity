using UnityEngine;

public class GoapDebugTester : MonoBehaviour
{
    // editor exposed
    [Header("References")]
    [Tooltip("If empty, will auto-find on this GameObject.")]
    [SerializeField] private GoapAgent m_agent;

    [Tooltip("If empty, will auto-find on this GameObject (or from GoapAgent).")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Behaviour")]
    [Tooltip("If true, reads the current world state into the fields every frame.")]
    [SerializeField] private bool m_autoRead;

    [Tooltip("If true, writes the fields into the world state every frame (overrides sensors!).")]
    [SerializeField] private bool m_autoApply;

    [Header("Current GOAP Int Values (World State)")]
    [Tooltip("0 means it isent alerted, 1 mean it is")]
    [SerializeField] private int m_alerted;               // 0/1
    [Tooltip("0 means target is not known, 1 means it is")]
    [SerializeField] private int m_targetKnown;           // 0/1
    [Tooltip("0 means target is not visible, 1 means it is")]
    [SerializeField] private int m_targetVisible;         // 0/1
    [Tooltip("0=None, 1=Far, 2=Chase, 3=Melee")]
    [SerializeField] private int m_targetRange;           // 0=None, 1=Far, 2=Chase, 3=Melee
    [Tooltip("0 means not facing target, 1 means it is")]
    [SerializeField] private int m_facingTarget;          // 0/1
    [Tooltip("0 means attack is on cooldown, 1 means ready")]
    [SerializeField] private int m_attackCooldownReady;   // 0/1
    [Tooltip("0 means target is not attacking now, 1 means it is")]
    [SerializeField] private int m_targetAttackingNow;    // 0/1
    [Tooltip("0 means no path to target, 1 means there is")]
    [SerializeField] private int m_hasPathToTarget;       // 0/1
    [Tooltip("0 means not blocking, 1 means it is")]
    [SerializeField] private int m_isBlocking;            // 0/1
    [Tooltip("0 means strong attack not primed, 1 means it is")]
    [SerializeField] private int m_strongPrimed;          // 0/1
    [Tooltip("Patrol stage index")]
    [SerializeField] private int m_patrolStage;           // int
    [Tooltip("Search stage index")]
    [SerializeField] private int m_searchStage;           // int
    [Tooltip("0..100 health percentage")]
    [SerializeField] private int m_selfHealth01;          // 0..100
    [Tooltip("0..100 target health percentage")]
    [SerializeField] private int m_targetHealth01;        // 0..100
    [Tooltip("0..100 incoming threat level")]
    [SerializeField] private int m_incomingThreat01;      // 0..100
    [Tooltip("Distance band index")]
    [SerializeField] private int m_distanceBand;          // int 
    [Tooltip("1 = , 2 = 3 = , 4= ")]
    [SerializeField] private int m_combatMode;            // int enum-ish
    [Tooltip("1 = , 2 = , 3 = ")]
    [SerializeField] private int m_pressureState;         // int enum-ish
    [Tooltip("0..100 cheese score")]
    [SerializeField] private int m_cheeseScore01;         // 0..100
    [Tooltip("0 none, 1 fire, 2 water, 3 wind/air")]
    [SerializeField] private int m_activeElement;         // 0 none, 1 fire, 2 water, 3 wind/air 
    [Tooltip(" how many fire charges (0..3)")]
    [SerializeField] private int m_fireCharges;           // 0..3
    [Tooltip(" how many water charges (0..3)")]
    [SerializeField] private int m_waterCharges;          // 0..3
    [Tooltip(" how many wind/air charges (0..3)")]
    [SerializeField] private int m_windCharges;           // 0..3
    [Tooltip("0 means charge point not known, 1 means it is")]
    [SerializeField] private int m_chargePointKnown;      // 0/1
    [Tooltip("0 means not at charge point, 1 means it is")]
    [SerializeField] private int m_atChargePoint;         // 0/1
    [Tooltip("0 means player not in lockout, 1 means it is")]
    [SerializeField] private int m_playerInLockout;       // 0/1
    [Tooltip("0 means not locked on, 1 means it is")]
    [SerializeField] private int m_isLockedOn;            // 0/1
    [Tooltip("0 none, 1 fire, 2 water, 3 air")]
    [SerializeField] private int m_desiredChargeElement;  // 0 none, 1 fire, 2 water, 3 air

    // public (read only)
    public GoapAgent Agent => m_agent;
    public GoapWorldStateBuilder WorldStateBuilder => m_worldStateBuilder;

    private void Awake()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    private void Update()
    {
        if (m_worldStateBuilder == null)
            return;

        if (m_autoRead)
            ReadFromAi();

        if (m_autoApply)
            ApplyToAi();
    }

    [ContextMenu("Read From AI")]
    public void ReadFromAi()
    {
        if (m_worldStateBuilder == null)
            return;

        GoapWorldState state = m_worldStateBuilder.WorldState;

        m_alerted = state.GetInt(GoapStateKey.Alerted);
        m_targetKnown = state.GetInt(GoapStateKey.TargetKnown);
        m_targetVisible = state.GetInt(GoapStateKey.TargetVisible);
        m_targetRange = state.GetInt(GoapStateKey.TargetRange);
        m_facingTarget = state.GetInt(GoapStateKey.FacingTarget);
        m_attackCooldownReady = state.GetInt(GoapStateKey.AttackCooldownReady);
        m_targetAttackingNow = state.GetInt(GoapStateKey.TargetAttackingNow);
        m_hasPathToTarget = state.GetInt(GoapStateKey.HasPathToTarget);
        m_isBlocking = state.GetInt(GoapStateKey.IsBlocking);
        m_strongPrimed = state.GetInt(GoapStateKey.StrongPrimed);
        m_patrolStage = state.GetInt(GoapStateKey.PatrolStage);
        m_searchStage = state.GetInt(GoapStateKey.SearchStage);

        m_selfHealth01 = state.GetInt(GoapStateKey.SelfHealth01);
        m_targetHealth01 = state.GetInt(GoapStateKey.TargetHealth01);
        m_incomingThreat01 = state.GetInt(GoapStateKey.IncomingThreat01);
        m_distanceBand = state.GetInt(GoapStateKey.DistanceBand);
        m_combatMode = state.GetInt(GoapStateKey.CombatMode);
        m_pressureState = state.GetInt(GoapStateKey.PressureState);
        m_cheeseScore01 = state.GetInt(GoapStateKey.CheeseScore01);

        m_activeElement = state.GetInt(GoapStateKey.ActiveElement);
        m_fireCharges = state.GetInt(GoapStateKey.FireCharges);
        m_waterCharges = state.GetInt(GoapStateKey.WaterCharges);
        m_windCharges = state.GetInt(GoapStateKey.WindCharges);

        m_chargePointKnown = state.GetInt(GoapStateKey.ChargePointKnown);
        m_atChargePoint = state.GetInt(GoapStateKey.AtChargePoint);
        m_playerInLockout = state.GetInt(GoapStateKey.PlayerInLockout);
        m_isLockedOn = state.GetInt(GoapStateKey.IsLockedOn);
        m_desiredChargeElement = state.GetInt(GoapStateKey.DesiredChargeElement);
    }

    [ContextMenu("Apply To AI")]
    public void ApplyToAi()
    {
        if (m_worldStateBuilder == null)
            return;

        // NOTE: This overrides sensors if m_autoApply is enabled.
        m_worldStateBuilder.SetInt(GoapStateKey.Alerted, m_alerted);
        m_worldStateBuilder.SetInt(GoapStateKey.TargetKnown, m_targetKnown);
        m_worldStateBuilder.SetInt(GoapStateKey.TargetVisible, m_targetVisible);
        m_worldStateBuilder.SetInt(GoapStateKey.TargetRange, m_targetRange);
        m_worldStateBuilder.SetInt(GoapStateKey.FacingTarget, m_facingTarget);
        m_worldStateBuilder.SetInt(GoapStateKey.AttackCooldownReady, m_attackCooldownReady);
        m_worldStateBuilder.SetInt(GoapStateKey.TargetAttackingNow, m_targetAttackingNow);
        m_worldStateBuilder.SetInt(GoapStateKey.HasPathToTarget, m_hasPathToTarget);
        m_worldStateBuilder.SetInt(GoapStateKey.IsBlocking, m_isBlocking);
        m_worldStateBuilder.SetInt(GoapStateKey.StrongPrimed, m_strongPrimed);
        m_worldStateBuilder.SetInt(GoapStateKey.PatrolStage, m_patrolStage);
        m_worldStateBuilder.SetInt(GoapStateKey.SearchStage, m_searchStage);

        m_worldStateBuilder.SetInt(GoapStateKey.SelfHealth01, m_selfHealth01);
        m_worldStateBuilder.SetInt(GoapStateKey.TargetHealth01, m_targetHealth01);
        m_worldStateBuilder.SetInt(GoapStateKey.IncomingThreat01, m_incomingThreat01);
        m_worldStateBuilder.SetInt(GoapStateKey.DistanceBand, m_distanceBand);
        m_worldStateBuilder.SetInt(GoapStateKey.CombatMode, m_combatMode);
        m_worldStateBuilder.SetInt(GoapStateKey.PressureState, m_pressureState);
        m_worldStateBuilder.SetInt(GoapStateKey.CheeseScore01, m_cheeseScore01);

        m_worldStateBuilder.SetInt(GoapStateKey.ActiveElement, m_activeElement);
        m_worldStateBuilder.SetInt(GoapStateKey.FireCharges, m_fireCharges);
        m_worldStateBuilder.SetInt(GoapStateKey.WaterCharges, m_waterCharges);
        m_worldStateBuilder.SetInt(GoapStateKey.WindCharges, m_windCharges);

        m_worldStateBuilder.SetInt(GoapStateKey.ChargePointKnown, m_chargePointKnown);
        m_worldStateBuilder.SetInt(GoapStateKey.AtChargePoint, m_atChargePoint);
        m_worldStateBuilder.SetInt(GoapStateKey.PlayerInLockout, m_playerInLockout);
        m_worldStateBuilder.SetInt(GoapStateKey.IsLockedOn, m_isLockedOn);
        m_worldStateBuilder.SetInt(GoapStateKey.DesiredChargeElement, m_desiredChargeElement);

        if (m_agent != null)
            m_agent.RequestReplan();
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_agent == null)
            m_agent = GetComponent<GoapAgent>();

        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_worldStateBuilder == null && m_agent != null)
            m_worldStateBuilder = m_agent.WorldStateBuilder;
    }
}
