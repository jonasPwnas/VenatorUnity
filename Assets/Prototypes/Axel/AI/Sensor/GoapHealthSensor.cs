using UnityEngine;

public interface IHealthReadOnly
{
    float Current { get; }
    float Max { get; }
}

[DisallowMultipleComponent]
public class GoapHealthSensor : MonoBehaviour, IGoapSensor
{
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    private IHealthReadOnly m_selfHealth;
    private IHealthReadOnly m_targetHealth;

    private void Awake()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();

        // self
        m_selfHealth = GetComponent<IHealthReadOnly>();

        // target: resolved every collect (cheap + robust)
    }

    public void Collect(GoapPerceptionBlackboard blackboard)
    {
        if (m_worldStateBuilder == null)
            return;

        if (m_selfHealth == null)
            m_selfHealth = GetComponent<IHealthReadOnly>();

        Transform t = m_worldStateBuilder.TargetTransform;
        if (t != null)
            m_targetHealth = t.GetComponentInParent<IHealthReadOnly>();

        int self01 = Read01(m_selfHealth);
        int target01 = Read01(m_targetHealth);

        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.SelfHealth01, self01);
        m_worldStateBuilder.SetIntIfChanged(GoapStateKey.TargetHealth01, target01);
    }

    private int Read01(IHealthReadOnly h)
    {
        if (h == null || h.Max <= 0.0001f)
            return 100;

        float t = Mathf.Clamp01(h.Current / h.Max);
        return Mathf.RoundToInt(t * 100f);
    }
}
