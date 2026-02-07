using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapUnitBrain : MonoBehaviour
{
    [Header("Designer")]
    [SerializeField] private GoapUnitTypeDefinition m_unitType;

    [Header("Auto Setup")]
    [SerializeField] private bool m_autoAddNavMeshAgent = true;
    [SerializeField] private bool m_autoAddSightSensor = true;
    [SerializeField] private bool m_autoAddNavMeshPathSensor = true;
    [SerializeField] private bool m_autoAddTouchSensor = true;
    

    private GoapAgent m_goapAgent;
    private GoapWorldStateBuilder m_worldStateBuilder;
    private GoapPerceptionHub m_perceptionHub;
    private GoapFovSightSensor m_sightSensor;
    private GoapNavMeshPathSensor m_navMeshPathSensor;
    private GoapTouchSensor m_touchSensor;
    private GoapActionLayer m_actionLayer;
    private GoapAttackCooldownDriver m_attackCooldownDriver;
    private GoapDebugTester m_debugTester;

    private void Reset()
    {
        AutoSetupComponents();
        ApplyUnitType();
    }

    private void OnValidate()
    {
        AutoSetupComponents();
        ApplyUnitType();
    }

    private void Awake()
    {
        AutoSetupComponents();
        ApplyUnitType();
    }

    private void AutoSetupComponents()
    {
        // Core execution
        m_actionLayer = GetComponent<GoapActionLayer>();
        if (m_actionLayer == null)
            m_actionLayer = gameObject.AddComponent<GoapActionLayer>();

        m_attackCooldownDriver = GetComponent<GoapAttackCooldownDriver>();
        if (m_attackCooldownDriver == null)
            m_attackCooldownDriver = gameObject.AddComponent<GoapAttackCooldownDriver>();

        if (m_autoAddNavMeshAgent && GetComponent<NavMeshAgent>() == null)
            gameObject.AddComponent<NavMeshAgent>();

        m_goapAgent = GetComponent<GoapAgent>();
        if (m_goapAgent == null)
            m_goapAgent = gameObject.AddComponent<GoapAgent>();

        m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = gameObject.AddComponent<GoapWorldStateBuilder>();

        m_perceptionHub = GetComponent<GoapPerceptionHub>();
        if (m_perceptionHub == null)
            m_perceptionHub = gameObject.AddComponent<GoapPerceptionHub>();

        if (GetComponent<GoapFacingSensor>() == null)
            gameObject.AddComponent<GoapFacingSensor>();

        if (GetComponent<GoapHealthSensor>() == null)
            gameObject.AddComponent<GoapHealthSensor>();

        if (GetComponent<GoapPlayerAttackSensor>() == null)
            gameObject.AddComponent<GoapPlayerAttackSensor>();

        if (GetComponent<GoapBlockingSensor>() == null)
            gameObject.AddComponent<GoapBlockingSensor>();

        if (GetComponent<GoapThreatSensor>() == null)
            gameObject.AddComponent<GoapThreatSensor>();

        if (GetComponent<GoapCheeseSensor>() == null)
            gameObject.AddComponent<GoapCheeseSensor>();

        // Combat context writers/controllers
        if (GetComponent<GoapCombatLocomotionController>() == null)
            gameObject.AddComponent<GoapCombatLocomotionController>();

        if (GetComponent<GoapCombatContextWriter>() == null)
            gameObject.AddComponent<GoapCombatContextWriter>();

        // Charging system helpers
        if (GetComponent<GoapChargeTargetContext>() == null)
            gameObject.AddComponent<GoapChargeTargetContext>();

        if (GetComponent<GoapElementChargePointSensor>() == null)
            gameObject.AddComponent<GoapElementChargePointSensor>();

        if (GetComponent<GoapChargeStepMemory>() == null)
            gameObject.AddComponent<GoapChargeStepMemory>();

        if (GetComponent<IElementInventory>() == null)
            gameObject.AddComponent<NpcElementInventory>();

        // Combat driver
        if (GetComponent<ICombatDriver>() == null)
            gameObject.AddComponent<EnemyKnightCombatDriver>();

        // Optional debugging
        m_debugTester = GetComponent<GoapDebugTester>();
        if (m_debugTester == null)
            m_debugTester = gameObject.AddComponent<GoapDebugTester>();

        // Classic sensors
        if (m_autoAddSightSensor)
        {
            m_sightSensor = GetComponent<GoapFovSightSensor>();
            if (m_sightSensor == null)
                m_sightSensor = gameObject.AddComponent<GoapFovSightSensor>();
        }

        if (m_autoAddNavMeshPathSensor)
        {
            m_navMeshPathSensor = GetComponent<GoapNavMeshPathSensor>();
            if (m_navMeshPathSensor == null)
                m_navMeshPathSensor = gameObject.AddComponent<GoapNavMeshPathSensor>();
        }

        if (m_autoAddTouchSensor)
        {
            m_touchSensor = GetComponent<GoapTouchSensor>();
            if (m_touchSensor == null)
                m_touchSensor = gameObject.AddComponent<GoapTouchSensor>();
        }
    }

    private void ApplyUnitType()
    {
        if (m_goapAgent == null || m_unitType == null)
            return;

        m_goapAgent.SetUnitType(m_unitType);
    }

    public void SetUnitType(GoapUnitTypeDefinition unitType)
    {
        m_unitType = unitType;
        ApplyUnitType();
    }

    public void OnDeath()
    {
        // Disable “brain”
        if (m_actionLayer != null) m_actionLayer.enabled = false;
        if (m_attackCooldownDriver != null) m_attackCooldownDriver.enabled = false;
        if (m_goapAgent != null) m_goapAgent.enabled = false;
        if (m_worldStateBuilder != null) m_worldStateBuilder.enabled = false;
        if (m_perceptionHub != null) m_perceptionHub.enabled = false;

        // Disable all sensors
        var sensors = GetComponents<MonoBehaviour>();
        for (int i = 0; i < sensors.Length; i++)
        {
            if (sensors[i] is IGoapSensor)
                sensors[i].enabled = false;
        }

        // Disable helpers/controllers
        var loco = GetComponent<GoapCombatLocomotionController>();
        if (loco != null) loco.enabled = false;

        var ctx = GetComponent<GoapCombatContextWriter>();
        if (ctx != null) ctx.enabled = false;

        var combat = GetComponent<EnemyKnightCombatDriver>();
        if (combat != null) combat.enabled = false;

        if (m_debugTester != null) m_debugTester.enabled = false;
    }
}
