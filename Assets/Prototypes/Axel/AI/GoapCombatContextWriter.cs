using UnityEngine;

[DisallowMultipleComponent]
public class GoapCombatContextWriter : MonoBehaviour
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private MonoBehaviour m_elementInventorySource;

    [Header("Update cadence")]
    [SerializeField] private float m_interval = 0.2f;

    [Header("Mode thresholds")]
    [SerializeField, Range(0,100)] private int m_lowHealthToDefensive01 = 35;
    [SerializeField, Range(0,100)] private int m_playerLowHealthToFinish01 = 25;
    [SerializeField, Range(0,100)] private int m_cheeseToAggro01 = 65;

    [Header("Element preferences")]
    [SerializeField] private int m_minChargesToStayElemental = 1;

    private IElementInventory m_elements;
    private float m_next;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_elementInventorySource == null)
            m_elementInventorySource = GetComponent<NpcElementInventory>();

        m_elements = m_elementInventorySource as IElementInventory;
        if (m_elements == null)
            m_elements = GetComponent<IElementInventory>();
    }

    private void Update()
    {
        if (m_worldStateBuilder == null)
            return;

        if (Time.time < m_next)
            return;

        m_next = Time.time + Mathf.Max(0.05f, m_interval);

        TickWrite();
    }

    private void TickWrite()
    {
        var ws = m_worldStateBuilder.WorldState;

        bool visible = ws.GetInt(GoapStateKey.TargetVisible, 0) == 1;
        int range = ws.GetInt(GoapStateKey.TargetRange, 0);
        int selfHp = ws.GetInt(GoapStateKey.SelfHealth01, 100);
        int playerHp = ws.GetInt(GoapStateKey.TargetHealth01, 100);
        int cheese = ws.GetInt(GoapStateKey.CheeseScore01, 0);

        int mode = (int)GoapCombatMode.None;
        int pressure = 0;

        // If we can see the player, choose combat mode
        if (visible)
        {
            if (playerHp <= m_playerLowHealthToFinish01)
            {
                mode = (int)GoapCombatMode.Aggressive;
                pressure = 1;
            }
            else if (cheese >= m_cheeseToAggro01)
            {
                mode = (int)GoapCombatMode.Aggressive;
                pressure = 1;
            }
            else if (selfHp <= m_lowHealthToDefensive01)
            {
                mode = (int)GoapCombatMode.Tactical;
                pressure = 0;
            }
            else
            {
                // default based on distance
                if (range == 3)
                {
                    mode = (int)GoapCombatMode.Aggressive;
                    pressure = 1;
                }
                else
                {
                    mode = (int)GoapCombatMode.Tactical;
                    pressure = 0;
                }
            }
        }

        ws.SetInt(GoapStateKey.CombatMode, mode);
        ws.SetInt(GoapStateKey.PressureState, pressure);

        // DesiredChargeElement (0 none, 1 fire, 2 water, 3 air/wind)
        // Simple rule: if low HP -> water, else if cheese -> fire, else if tactical -> wind
        int desired = 0;
        if (selfHp <= m_lowHealthToDefensive01) desired = 2;
        else if (cheese >= m_cheeseToAggro01) desired = 1;
        else if (mode == (int)GoapCombatMode.Tactical) desired = 3;

        ws.SetInt(GoapStateKey.DesiredChargeElement, desired);

        if (m_elements != null && desired != 0)
        {
            // Map int->ElementTypes.ElementType
            ElementTypes.ElementType e = ElementTypes.ElementType.NoElement;
            if (desired == 1) e = ElementTypes.ElementType.Fire;
            if (desired == 2) e = ElementTypes.ElementType.Water;
            if (desired == 3) e = ElementTypes.ElementType.Air;

            if (m_elements.GetCharges(e) >= m_minChargesToStayElemental)
                m_elements.SetActiveElement(e);
        }
    }
}
