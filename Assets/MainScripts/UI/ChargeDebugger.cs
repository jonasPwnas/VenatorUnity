using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeDebugger : MonoBehaviour
{
    [SerializeField] private ChargeManager m_hudManager;
    public InputAction AddCharge;
    public InputAction RemoveCharge;

    // Параметры для теста / Parameters for testing
    [SerializeField] public int m_maxCharges = 3;
    [SerializeField] public int m_currentCharges = 0;

    private void Start()
    {
        if (m_hudManager == null)
            m_hudManager = Object.FindFirstObjectByType<ChargeManager>();

        UpdateHud();
    }

    private void OnEnable()
    {
        AddCharge.Enable();
        RemoveCharge.Enable();
    }

    private void OnDisable()
    {
        RemoveCharge.Disable();
        AddCharge.Disable();
    }

    private void Update()
    {
        if (AddCharge.triggered)
        {
            if (m_currentCharges < m_maxCharges)
            {
                m_currentCharges++;
                UpdateHud();
            }
        }

        if (RemoveCharge.triggered)
        {
            if (m_currentCharges > 0)
            {
                m_currentCharges--;
                UpdateHud();
            }
        }
    }

    private void UpdateHud()
    {
        m_hudManager.UpdateDisplay(m_maxCharges, m_currentCharges);
    }
}
