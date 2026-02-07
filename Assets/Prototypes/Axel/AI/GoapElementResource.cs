using UnityEngine;

public enum ElementType
{
    None = 0,
    Fire = 1,
    Water = 2,
    Wind = 3
}

[DisallowMultipleComponent]
public class GoapElementResource : MonoBehaviour
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("State")]
    [SerializeField] private ElementType m_activeElement = ElementType.None;
    [SerializeField, Range(0, 3)] private int m_fireCharges = 0;
    [SerializeField, Range(0, 3)] private int m_waterCharges = 0;
    [SerializeField, Range(0, 3)] private int m_windCharges = 0;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        PushToWorld();
    }

    public ElementType ActiveElement => m_activeElement;
    public int FireCharges => m_fireCharges;
    public int WaterCharges => m_waterCharges;
    public int WindCharges => m_windCharges;

    public void SetActiveElement(ElementType element)
    {
        m_activeElement = element;
        PushToWorld();
    }

    public bool CanSpend(ElementType element, int amount)
    {
        int c = GetCharges(element);
        return c >= amount;
    }

    public bool TrySpend(ElementType element, int amount)
    {
        if (!CanSpend(element, amount))
            return false;

        SetCharges(element, GetCharges(element) - amount);
        PushToWorld();
        return true;
    }

    public void AddCharges(ElementType element, int amount)
    {
        int next = Mathf.Clamp(GetCharges(element) + amount, 0, 3);
        SetCharges(element, next);
        PushToWorld();
    }

    public void RefillAll()
    {
        m_fireCharges = 3;
        m_waterCharges = 3;
        m_windCharges = 3;
        PushToWorld();
    }

    private int GetCharges(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return m_fireCharges;
            case ElementType.Water: return m_waterCharges;
            case ElementType.Wind: return m_windCharges;
            default: return 0;
        }
    }

    private void SetCharges(ElementType element, int value)
    {
        switch (element)
        {
            case ElementType.Fire: m_fireCharges = value; break;
            case ElementType.Water: m_waterCharges = value; break;
            case ElementType.Wind: m_windCharges = value; break;
        }
    }

    private void PushToWorld()
    {
        if (m_worldStateBuilder == null)
            return;

        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.ActiveElement, (int)m_activeElement);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.FireCharges, m_fireCharges);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.WaterCharges, m_waterCharges);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.WindCharges, m_windCharges);
    }
}
