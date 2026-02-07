using UnityEngine;

[DisallowMultipleComponent]
public class GoapAnimationEventRelay : MonoBehaviour
{
    [Tooltip("If empty, will auto-find GoapActionLayer in parents.")]
    [SerializeField] private GoapActionLayer m_actionLayer;

    private void Awake()
    {
        AutoGetReferencesIfMissing();
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    public void AnimEvent_Done()
    {
        if (m_actionLayer == null)
            return;

        m_actionLayer.AnimEvent_Done();
    }

    public void AnimEvent_LookDone()
    {
        if (m_actionLayer == null)
            return;

        m_actionLayer.AnimEvent_LookDone();
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_actionLayer == null)
            m_actionLayer = GetComponentInParent<GoapActionLayer>();
    }
}
