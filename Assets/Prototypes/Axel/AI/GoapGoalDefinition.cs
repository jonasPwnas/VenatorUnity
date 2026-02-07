using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Goal Definition")]
public class GoapGoalDefinition : ScriptableObject
{
    [Header("Priority")]
    [SerializeField] private int m_basePriority = 50;

    [Tooltip("Extra score added if this goal is already active (reduces thrashing).")]
    [SerializeField] private int m_stickinessBonus = 10;

    [Tooltip("New goal must beat current goal by this much to switch.")]
    [SerializeField] private int m_hysteresis = 10;

    [Tooltip("Seconds after completion before this goal is eligible again.")]
    [SerializeField] private float m_cooldownSeconds = 0.5f;

    [Header("Eligibility Gate")]
    [SerializeField] private GoapCondition[] m_required;

    [Header("Satisfied When")]
    [SerializeField] private GoapCondition[] m_desired;

    [Header("Dynamic Scoring")]
    [SerializeField] private GoapScoreRule[] m_scoreRules;

    [Header("Completion")]
    [Tooltip("Applied to the LIVE world state once when the goal is completed.")]
    [SerializeField] private GoapEffect[] m_onCompletedEffects;

    public GoapEffect[] OnCompletedEffects => m_onCompletedEffects;
    public int BasePriority => m_basePriority;
    public int StickinessBonus => m_stickinessBonus;
    public int Hysteresis => m_hysteresis;
    public float CooldownSeconds => m_cooldownSeconds;

    public GoapCondition[] Required => m_required;
    public GoapCondition[] Desired => m_desired;
    public GoapScoreRule[] ScoreRules => m_scoreRules;

    public bool IsSatisfied(GoapWorldState worldState)
    {
        if (m_desired == null || m_desired.Length == 0)
            return false;

        for (int i = 0; i < m_desired.Length; i++)
        {
            if (!m_desired[i].Evaluate(worldState))
                return false;
        }

        return true;
    }

    public bool IsEligible(GoapWorldState worldState, float timeNow, float lastCompletedTime)
    {
        if (timeNow - lastCompletedTime < m_cooldownSeconds)
            return false;

        if (m_required == null)
            return true;

        for (int i = 0; i < m_required.Length; i++)
        {
            if (!m_required[i].Evaluate(worldState))
                return false;
        }

        return true;
    }

    public int ComputeScore(GoapWorldState worldState, bool isCurrentGoal)
    {
        int score = m_basePriority;

        if (m_scoreRules != null)
        {
            for (int i = 0; i < m_scoreRules.Length; i++)
                score += m_scoreRules[i].Evaluate(worldState);
        }

        if (isCurrentGoal)
            score += m_stickinessBonus;

        return score;
    }
}
