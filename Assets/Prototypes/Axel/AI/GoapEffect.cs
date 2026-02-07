using System;
using UnityEngine;

[Serializable]
public struct GoapEffect
{
    [SerializeField] private GoapStateKey m_key;
    [SerializeField] private GoapEffectOp m_op;
    [SerializeField] private int m_intValue;

    public void Apply(GoapWorldState worldState)
    {
        int current = worldState.GetInt(m_key);

        switch (m_op)
        {
            case GoapEffectOp.Set:
                worldState.SetInt(m_key, m_intValue);
                break;

            case GoapEffectOp.Add:
                worldState.SetInt(m_key, current + m_intValue);
                break;

            case GoapEffectOp.Subtract:
                worldState.SetInt(m_key, current - m_intValue);
                break;
        }
    }
}
