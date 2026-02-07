using UnityEngine;

public class GoapWorldStateBuilder : MonoBehaviour
{
    [Header("Debug (Read Only)")]
    [SerializeField] private GoapWorldState m_worldState = new GoapWorldState();

    [Header("Target Info")]
    [SerializeField] private Transform m_targetTransform;
    [SerializeField] private Vector3 m_lastKnownTargetPosition;

    [Header("Charge Point Info")]
    [SerializeField] private Vector3 m_chargePointPosition;
    [SerializeField] private bool m_hasChargePoint;

    public GoapWorldState WorldState => m_worldState;
    public Transform TargetTransform => m_targetTransform;
    public Vector3 LastKnownTargetPosition => m_lastKnownTargetPosition;

    public bool HasChargePoint => m_hasChargePoint;
    public Vector3 ChargePointPosition => m_chargePointPosition;

    private void Awake()
    {
        m_worldState.InitializeIfNeeded();

        if (m_targetTransform == null)
            m_targetTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void SetTargetTransform(Transform target)
    {
        m_targetTransform = target;
    }

    public void SetLastKnownTargetPosition(Vector3 position)
    {
        m_lastKnownTargetPosition = position;
    }

    public void SetChargePoint(Vector3 position)
    {
        m_chargePointPosition = position;
        m_hasChargePoint = true;
    }

    public void ClearChargePoint()
    {
        m_hasChargePoint = false;
        m_chargePointPosition = Vector3.zero;
    }

    public void SetBool(GoapStateKey key, bool value)
    {
        m_worldState.SetBool(key, value);
    }

    public void SetInt(GoapStateKey key, int value)
    {
        m_worldState.SetInt(key, value);
    }

    public void SetIntIfChanged(GoapStateKey key, int value)
    {
        if (m_worldState.GetInt(key, 0) == value)
            return;
        //Debug.Log($"World State Change: {key} set to {value}");
        m_worldState.SetInt(key, value);
    }

    public void SetTargetRangeInt(int rangeValue)
    {
        SetIntIfChanged(GoapStateKey.TargetRange, rangeValue);
    }
}
