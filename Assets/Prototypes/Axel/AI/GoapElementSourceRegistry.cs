using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapElementSourceRegistry : MonoBehaviour
{
    public static GoapElementSourceRegistry Instance { get; private set; }

    [Header("Scan Settings")]
    [Tooltip("Only sources on these layers are considered charge sources.")]
    [SerializeField] private LayerMask m_sourceLayerMask;

    [Tooltip("If true, includes inactive objects in the scan.")]
    [SerializeField] private bool m_includeInactive = true;

    private readonly List<ParentElementSource> m_fireSources = new List<ParentElementSource>(32);
    private readonly List<ParentElementSource> m_waterSources = new List<ParentElementSource>(32);

    private void Awake()
    {
        Instance = this;
        Refresh();
        //Debug.Log("[GoapElementSourceRegistry] Initialized with " + $"{m_fireSources.Count} fire sources and {m_waterSources.Count} water sources.");
    }

    public void Refresh()
    {
        m_fireSources.Clear();
        m_waterSources.Clear();

        ParentElementSource[] all = FindObjectsByType<ParentElementSource>(
            m_includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < all.Length; i++)
        {
            ParentElementSource s = all[i];
            if (s == null)
                continue;

            if (!IsInMask(s.gameObject.layer, m_sourceLayerMask))
                continue;

            ElementTypes.ElementType e = s.GetSourceElement();
            if (e == ElementTypes.ElementType.Fire)
                m_fireSources.Add(s);
            else if (e == ElementTypes.ElementType.Water)
                m_waterSources.Add(s);
        }
    }

    public bool TryGetNearestReachable(
        NavMeshAgent agent,
        Vector3 from,
        ElementTypes.ElementType element,
        out ParentElementSource best)
    {
        best = null;

        if (agent == null)
            return false;

        List<ParentElementSource> list = GetList(element);
        if (list == null || list.Count == 0)
            return false;

        NavMeshPath path = new NavMeshPath();

        float bestDist = float.MaxValue;

        for (int i = 0; i < list.Count; i++)
        {
            ParentElementSource s = list[i];
            if (s == null)
                continue;

            Vector3 p = s.transform.position;

            // quick distance check
            float d = (p - from).sqrMagnitude;
            if (d >= bestDist)
                continue;

            // path check
            bool ok = agent.CalculatePath(p, path);
            if (!ok || path.status != NavMeshPathStatus.PathComplete)
                continue;

            bestDist = d;
            best = s;
        }

        return best != null;
    }

    private List<ParentElementSource> GetList(ElementTypes.ElementType element)
    {
        switch (element)
        {
            case ElementTypes.ElementType.Fire: return m_fireSources;
            case ElementTypes.ElementType.Water: return m_waterSources;
        }

        return null;
    }

    private bool IsInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
