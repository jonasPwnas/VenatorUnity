using System.Collections.Generic;
using UnityEngine;

public class GoapActionRuntimeMemory : MonoBehaviour
{
    private struct Entry
    {
        public float StartTime;
        public bool StartedOk;
        public bool Success;
        public bool HasResult;
    }

    //Test


    private readonly Dictionary<Object, Entry> m_entries = new Dictionary<Object, Entry>(64);

    public void SetStart(Object key, float startTime, bool startedOk)
    {
        m_entries[key] = new Entry
        {
            StartTime = startTime,
            StartedOk = startedOk,
            Success = startedOk,
            HasResult = false
        };
    }

    public bool TryGetStart(Object key, out float startTime, out bool startedOk)
    {
        startTime = 0f;
        startedOk = false;

        if (!m_entries.TryGetValue(key, out Entry e))
            return false;

        startTime = e.StartTime;
        startedOk = e.StartedOk;
        return true;
    }

    public void SetResult(Object key, bool success)
    {
        if (!m_entries.TryGetValue(key, out Entry e))
            e = new Entry();

        e.Success = success;
        e.HasResult = true;
        m_entries[key] = e;
    }

    public bool TryGetResult(Object key, out bool hasResult, out bool success)
    {
        hasResult = false;
        success = true;

        if (!m_entries.TryGetValue(key, out Entry e))
            return false;

        hasResult = e.HasResult;
        success = e.Success;
        return true;
    }

    public void Clear(Object key)
    {
        if (m_entries.ContainsKey(key))
            m_entries.Remove(key);
    }
}
