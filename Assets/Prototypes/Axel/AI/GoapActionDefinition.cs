using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Action Definition")]
public class GoapActionDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Used for hooking into your other systems. If empty, asset name is used.")]
    [SerializeField] private string m_actionId;

    [Header("Planning")]
    [SerializeField] private int m_baseCost = 10;
    [SerializeField] private GoapCondition[] m_preconditions;
    [SerializeField] private GoapEffect[] m_effects;

    [Header("Execution")]
    [SerializeField] private GoapActionLogic m_logic;

    public string ActionId => string.IsNullOrEmpty(m_actionId) ? name : m_actionId;
    public int BaseCost => m_baseCost;
    public GoapCondition[] Preconditions => m_preconditions;
    public GoapEffect[] Effects => m_effects;
    public GoapActionLogic Logic => m_logic;
}
