using UnityEngine;

[DisallowMultipleComponent]
public class GoapChargeTargetContext : MonoBehaviour
{
    [SerializeField] private ParentElementSource m_currentSource;

    public ParentElementSource CurrentSource => m_currentSource;

    public void SetSource(ParentElementSource source)
    {
        m_currentSource = source;
    }

    public void Clear()
    {
        m_currentSource = null;
    }
}
