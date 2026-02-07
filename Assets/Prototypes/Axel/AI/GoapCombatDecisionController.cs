using UnityEngine;

[DisallowMultipleComponent]
public class GoapCombatDecisionController : MonoBehaviour
{
    [SerializeField] private bool m_isEnabled = true;

    [Header("Cadence")]
    [SerializeField] private float m_decisionInterval = 0.12f;

    [Header("Special settings")]
    [SerializeField] private int m_fireCost = 2;
    [SerializeField] private int m_waterCost = 1;
    [SerializeField] private int m_windCost = 1;

    private float m_nextDecisionTime;

    private GoapWorldStateBuilder m_worldStateBuilder;
    private GoapElementResource m_elements;
    private ICombatDriver m_combat;

    private void Awake()
    {
        m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
        m_elements = GetComponent<GoapElementResource>();
        m_combat = GetComponent<ICombatDriver>();
    }

    private void Update()
    {
        if (!m_isEnabled)
            return;

        if (Time.time < m_nextDecisionTime)
            return;

        m_nextDecisionTime = Time.time + Mathf.Max(0.05f, m_decisionInterval);

        TickDecision();
    }

    private void TickDecision()
    {
        if (m_worldStateBuilder == null || m_combat == null)
            return;

        GoapWorldState ws = m_worldStateBuilder.WorldState;

        bool visible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);
        int facing = ws.GetInt(GoapStateKey.FacingTarget, 0);
        int mode = ws.GetInt(GoapStateKey.CombatMode, 0);
        int pressure = ws.GetInt(GoapStateKey.PressureState, 0);
        int playerLockout = ws.GetInt(GoapStateKey.PlayerInLockout, 0);

        if (!visible)
            return;

        // Always lock-on when fighting
        m_combat.SetLockOn(true, m_worldStateBuilder.TargetTransform);

        // If not in melee, don't attack here (GOAP handles movement)
        if (range != 3 || facing == 0)
            return;

        if (m_combat.IsBusy)
        {
            // If combo window open and we want pressure, continue light combo sometimes
            if (m_combat.IsComboWindowOpen && pressure == 1)
                m_combat.TryLightAttack();

            return;
        }

        // Priority punish: player lockout
        if (playerLockout == 1)
        {
            // Heavy punish is strong
            m_combat.TryHeavyAttack();
            return;
        }

        // Specials based on mode + element + charges
        if (TrySpecial(mode))
            return;

        // Attack selection
        if (pressure == 1)
        {
            // Pressure: favor light combo; occasionally heavy
            if (Random.value < 0.75f)
                m_combat.TryLightAttack();
            else
                m_combat.TryHeavyAttack();
        }
        else
        {
            // Reset: be cautious, poke once or do nothing
            if (Random.value < 0.35f)
                m_combat.TryLightAttack();
        }
    }

    private bool TrySpecial(int combatMode)
    {
        if (m_elements == null)
            return false;

        ElementType element = m_elements.ActiveElement;

        // Elemental mode prefers specials if we can afford them and we are not busy
        bool prefer = (combatMode == (int)GoapCombatMode.Elemental);

        if (!prefer)
            return false;

        if (element == ElementType.Fire && m_elements.CanSpend(ElementType.Fire, m_fireCost))
        {
            if (m_combat.TryCastFireAoE())
            {
                m_elements.TrySpend(ElementType.Fire, m_fireCost);
                return true;
            }
        }

        if (element == ElementType.Water && m_elements.CanSpend(ElementType.Water, m_waterCost))
        {
            if (m_combat.TryCastWaterHeal())
            {
                m_elements.TrySpend(ElementType.Water, m_waterCost);
                return true;
            }
        }

        if (element == ElementType.Wind && m_elements.CanSpend(ElementType.Wind, m_windCost))
        {
            if (m_combat.TryCastWindPush())
            {
                m_elements.TrySpend(ElementType.Wind, m_windCost);
                return true;
            }
        }

        return false;
    }
}
