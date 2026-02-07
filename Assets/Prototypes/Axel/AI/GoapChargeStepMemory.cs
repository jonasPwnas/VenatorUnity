using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GoapChargeStepMemory : MonoBehaviour
{
    private readonly Dictionary<Object, int> m_steps = new Dictionary<Object, int>(16);

    public int GetApplied(Object key)
    {
        if (key == null)
            return 0;

        return m_steps.TryGetValue(key, out int v) ? v : 0;
    }

    public void SetApplied(Object key, int v)
    {
        if (key == null)
            return;

        m_steps[key] = v;
    }

    public void Clear(Object key)
    {
        if (key == null)
            return;

        if (m_steps.ContainsKey(key))
            m_steps.Remove(key);
    }
}
