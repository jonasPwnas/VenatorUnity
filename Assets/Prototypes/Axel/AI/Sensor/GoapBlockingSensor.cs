using UnityEngine;

[DisallowMultipleComponent]
public class GoapBlockingSensor : MonoBehaviour, IGoapSensor
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private MonoBehaviour m_combatDriverSource;

    private ICombatDriver m_combat;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_combatDriverSource == null)
            m_combatDriverSource = GetComponent<MonoBehaviour>();

        m_combat = GetComponent<ICombatDriver>();
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (m_worldStateBuilder == null)
            return;

        if (m_combat == null)
            m_combat = GetComponent<ICombatDriver>();

        bool blocking = m_combat != null && m_combat.IsBlocking;
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.IsBlocking, blocking ? 1 : 0);
    }
}
