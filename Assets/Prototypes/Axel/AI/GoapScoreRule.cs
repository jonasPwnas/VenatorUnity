using System;
using UnityEngine;

public enum GoapScoreRuleType
{
    AddIfConditionTrue,
    AddIfConditionFalse,
    AddByCurve01FromInt01 // reads int as 0..100, converts to 0..1 then eval curve
}

[Serializable]
public struct GoapScoreRule
{
    [SerializeField] private GoapScoreRuleType m_ruleType;

    [Header("For condition-based rules")]
    [SerializeField] private GoapCondition m_condition;
    [SerializeField] private int m_addScore;

    [Header("For curve-based rules")]
    [SerializeField] private GoapStateKey m_sourceKey;
    [SerializeField] private AnimationCurve m_curve;
    [SerializeField] private float m_curveMultiplier;

    public int Evaluate(GoapWorldState worldState)
    {
        switch (m_ruleType)
        {
            case GoapScoreRuleType.AddIfConditionTrue:
                return m_condition.Evaluate(worldState) ? m_addScore : 0;

            case GoapScoreRuleType.AddIfConditionFalse:
                return m_condition.Evaluate(worldState) ? 0 : m_addScore;

            case GoapScoreRuleType.AddByCurve01FromInt01:
                int raw = worldState.GetInt(m_sourceKey, 0);
                float t = Mathf.Clamp01(raw / 100f);

                if (m_curve == null)
                    return 0;

                float curveValue = m_curve.Evaluate(t) * m_curveMultiplier;
                return Mathf.RoundToInt(curveValue);
        }

        return 0;
    }
}
