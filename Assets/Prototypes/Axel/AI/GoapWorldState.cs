using System;
using UnityEngine;

[Serializable]
public class GoapWorldState
{
    [SerializeField] private int[] m_values;

    private static int KEY_COUNT => Enum.GetValues(typeof(GoapStateKey)).Length;

    private void EnsureSize()
    {
        if (m_values == null || m_values.Length != KEY_COUNT)
            Array.Resize(ref m_values, KEY_COUNT);
    }

    public void InitializeIfNeeded()
    {
        EnsureSize();
    }

    public int GetInt(GoapStateKey key, int defaultValue = 0)
    {
        EnsureSize();
        return m_values[(int)key];
    }

    public bool GetBool(GoapStateKey key)
    {
        return GetInt(key, 0) != 0;
    }

    public void SetInt(GoapStateKey key, int value)
    {
        EnsureSize();
        m_values[(int)key] = value;
    }

    public void SetBool(GoapStateKey key, bool value)
    {
        SetInt(key, value ? 1 : 0);
    }

    public GoapWorldState Clone()
    {
        EnsureSize();
        GoapWorldState clone = new GoapWorldState();
        clone.m_values = (int[])m_values.Clone();
        return clone;
    }

    public ulong GetHash64()
    {
        EnsureSize();

        // Fast hash: FNV-1a-ish
        ulong h = 14695981039346656037UL;
        unchecked
        {
            for (int i = 0; i < m_values.Length; i++)
            {
                h ^= (ulong)(m_values[i] + (i * 1000003));
                h *= 1099511628211UL;
            }
        }
        return h;
    }
}
