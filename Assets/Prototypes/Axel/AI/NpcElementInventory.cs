using UnityEngine;

public interface IElementInventory
{
    ElementTypes.ElementType ActiveElement { get; }
    int GetCharges(ElementTypes.ElementType element);

    void SetActiveElement(ElementTypes.ElementType element);

    bool CanSpend(ElementTypes.ElementType element, int amount);
    bool TrySpend(ElementTypes.ElementType element, int amount);

    void AddCharges(ElementTypes.ElementType element, int amount);
}

[DisallowMultipleComponent]
public class NpcElementInventory : MonoBehaviour, IElementInventory
{
    [Header("References")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("State")]
    [SerializeField] private ElementTypes.ElementType m_activeElement = ElementTypes.ElementType.NoElement;

    [SerializeField, Range(0, 3)] private int m_fireCharges = 3;
    [SerializeField, Range(0, 3)] private int m_waterCharges = 3;
    [SerializeField, Range(0, 3)] private int m_airCharges = 3;


    public bool ALwayPushWorldState = true;

    private void Update()
    {
        if(ALwayPushWorldState)
            PushWorld();
    }

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        PushWorld();
    }

    public ElementTypes.ElementType ActiveElement => m_activeElement;

    public int GetCharges(ElementTypes.ElementType element)
    {
        switch (element)
        {
            case ElementTypes.ElementType.Fire: return m_fireCharges;
            case ElementTypes.ElementType.Water: return m_waterCharges;
            case ElementTypes.ElementType.Air: return m_airCharges;
        }

        return 0;
    }

    public void SetActiveElement(ElementTypes.ElementType element)
    {
        m_activeElement = element;
        PushWorld();
    }

    public bool CanSpend(ElementTypes.ElementType element, int amount)
    {
        return GetCharges(element) >= amount;
    }

    public bool TrySpend(ElementTypes.ElementType element, int amount)
    {
        if (!CanSpend(element, amount))
            return false;

        SetCharges(element, GetCharges(element) - amount);
        PushWorld();
        return true;
    }

    public void AddCharges(ElementTypes.ElementType element, int amount)
    {
        int next = Mathf.Clamp(GetCharges(element) + amount, 0, 3);
        SetCharges(element, next);
        PushWorld();
    }

    private void SetCharges(ElementTypes.ElementType element, int v)
    {
        v = Mathf.Clamp(v, 0, 3);

        switch (element)
        {
            case ElementTypes.ElementType.Fire: m_fireCharges = v; break;
            case ElementTypes.ElementType.Water: m_waterCharges = v; break;
            case ElementTypes.ElementType.Air: m_airCharges = v; break;
        }
    }

    private void PushWorld()
    {
        if(m_worldStateBuilder == null)
            Debug.Log("NpcElementInventory requires a GoapWorldStateBuilder to push world state.");
        if (m_worldStateBuilder == null)
            return;

        // ActiveElement mapping: 0 none, 1 fire, 2 water, 3 air
        int active = MapElementToInt(m_activeElement);

        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.ActiveElement, active);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.FireCharges, m_fireCharges);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.WaterCharges, m_waterCharges);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.WindCharges, m_airCharges);
    }

    private int MapElementToInt(ElementTypes.ElementType e)
    {
        switch (e)
        {
            case ElementTypes.ElementType.Fire: return 1;
            case ElementTypes.ElementType.Water: return 2;
            case ElementTypes.ElementType.Air: return 3;
        }

        return 0;
    }
}
