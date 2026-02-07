using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapElementChargePointSensor : MonoBehaviour, IGoapSensor
{
    [Header("References")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;
    [SerializeField] private GoapChargeTargetContext m_chargeContext;
    [SerializeField] private NavMeshAgent m_navMeshAgent;
    [SerializeField] private MonoBehaviour m_elementInventorySource;

    [Header("Behavior")]
    [Tooltip("How often to re-evaluate best charge source (seconds).")]
    [SerializeField] private float m_refreshInterval = 0.75f;

    [Tooltip("How close counts as 'at charge point'.")]
    [SerializeField] private float m_atPointDistance = 1.4f;

    private IElementInventory m_elements;
    private float m_nextRefreshTime;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        if (m_chargeContext == null)
            m_chargeContext = GetComponent<GoapChargeTargetContext>();

        if (m_navMeshAgent == null)
            m_navMeshAgent = GetComponent<NavMeshAgent>();

        if (m_elementInventorySource == null)
            m_elementInventorySource = GetComponent<NpcElementInventory>();

        m_elements = m_elementInventorySource as IElementInventory;
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (m_worldStateBuilder == null || m_navMeshAgent == null || m_elements == null || m_chargeContext == null)
            return;

        // Decide which element we want to charge:
        // Prefer DesiredChargeElement if you added it; otherwise fall back to ActiveElement.
        GoapWorldState ws = m_worldStateBuilder.WorldState;

        int desired = ws.GetInt(GoapStateKey.DesiredChargeElement, 0);
        ElementTypes.ElementType desiredElement = MapIntToElement(desired);

        if (desiredElement == ElementTypes.ElementType.NoElement)
            desiredElement = m_elements.ActiveElement;

        // Air is charge-anywhere: no point needed.
        if (desiredElement == ElementTypes.ElementType.Air || desiredElement == ElementTypes.ElementType.NoElement)
        {
            m_chargeContext.Clear();
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.ChargePointKnown, 0);
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.AtChargePoint, 0);
            m_worldStateBuilder.ClearChargePoint();
            return;
        }

        if (Time.time >= m_nextRefreshTime)
        {
            m_nextRefreshTime = Time.time + Mathf.Max(0.1f, m_refreshInterval);
            RefreshBestSource(desiredElement);
        }

        ParentElementSource src = m_chargeContext.CurrentSource;
        if (src == null)
        {
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.ChargePointKnown, 0);
            m_worldStateBuilder.SetIntIfChanged(GoapStateKey.AtChargePoint, 0);
            m_worldStateBuilder.ClearChargePoint();
            return;
        }

        Vector3 p = src.transform.position;
        m_worldStateBuilder.SetChargePoint(p);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.ChargePointKnown, 1);

        float dist = Vector3.Distance(transform.position, p);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.AtChargePoint, dist <= m_atPointDistance ? 1 : 0);
    }

    private void RefreshBestSource(ElementTypes.ElementType element)
    {
        if (GoapElementSourceRegistry.Instance == null)
            return;

        if (!GoapElementSourceRegistry.Instance.TryGetNearestReachable(m_navMeshAgent, transform.position, element, out ParentElementSource best))
        {
            m_chargeContext.Clear();
            return;
        }

        m_chargeContext.SetSource(best);
    }

    private ElementTypes.ElementType MapIntToElement(int v)
    {
        switch (v)
        {
            case 1: return ElementTypes.ElementType.Fire;
            case 2: return ElementTypes.ElementType.Water;
            case 3: return ElementTypes.ElementType.Air;
        }

        return ElementTypes.ElementType.NoElement;
    }
}
