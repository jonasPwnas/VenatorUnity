using UnityEngine;

[DisallowMultipleComponent]
public class GoapCombatReflexController : MonoBehaviour
{
    [SerializeField] private bool m_isEnabled = true;

    [Header("Thresholds")]
    [SerializeField, Range(0, 100)] private int m_threatToBlock01 = 60;
    [SerializeField, Range(0, 100)] private int m_threatToRoll01 = 90;

    private GoapWorldStateBuilder m_worldStateBuilder;
    private ICombatDriver m_combat;

    private void Awake()
    {
        m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
        m_combat = GetComponent<ICombatDriver>();
    }

    private void Update()
    {
        if (!m_isEnabled)
            return;

        if (m_worldStateBuilder == null || m_combat == null)
            return;

        GoapWorldState ws = m_worldStateBuilder.WorldState;

        bool visible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);
        int threat = ws.GetInt(GoapStateKey.IncomingThreat01, 0);

        if (!visible || range != 3)
        {
            m_combat.SetBlocking(false);
            return;
        }

        if (threat >= m_threatToRoll01)
        {
            if (!m_combat.IsBusy)
            {
                m_combat.SetBlocking(false);
                m_combat.TryRoll();
            }
            return;
        }

        if (threat >= m_threatToBlock01)
        {
            m_combat.SetBlocking(true);
            return;
        }

        // Not threatened
        m_combat.SetBlocking(false);
    }
}
