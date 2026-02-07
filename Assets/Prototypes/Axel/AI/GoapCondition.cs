using System;
using UnityEngine;

[Serializable]
public struct GoapCondition
{
    [SerializeField] private GoapStateKey m_key;
    [SerializeField] private GoapCompareOp m_op;
    [SerializeField] private int m_intValue;

    public GoapStateKey Key => m_key;

    public bool Evaluate(GoapWorldState worldState)
    {
        int current = worldState.GetInt(m_key);

        switch (m_op)
        {
            case GoapCompareOp.Equals:
                return current == m_intValue;

            case GoapCompareOp.NotEquals:
                return current != m_intValue;

            case GoapCompareOp.Greater:
                return current > m_intValue;

            case GoapCompareOp.Less:
                return current < m_intValue;

            case GoapCompareOp.IsTrue:
                return current != 0;

            case GoapCompareOp.IsFalse:
                return current == 0;
        }

        return false;
    }
}
