#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GoapAssetCache
{
    private static List<GoapActionDefinition> m_actions;
    private static List<GoapGoalDefinition> m_goals;
    private static List<GoapUnitTypeDefinition> m_unitTypes;

    public static void Refresh()
    {
        m_actions = FindAssetsOfType<GoapActionDefinition>();
        m_goals = FindAssetsOfType<GoapGoalDefinition>();
        m_unitTypes = FindAssetsOfType<GoapUnitTypeDefinition>();
    }

    public static List<GoapActionDefinition> Actions
    {
        get
        {
            if (m_actions == null) Refresh();
            return m_actions;
        }
    }

    public static List<GoapGoalDefinition> Goals
    {
        get
        {
            if (m_goals == null) Refresh();
            return m_goals;
        }
    }

    public static List<GoapUnitTypeDefinition> UnitTypes
    {
        get
        {
            if (m_unitTypes == null) Refresh();
            return m_unitTypes;
        }
    }

    private static List<T> FindAssetsOfType<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        List<T> list = new List<T>(guids.Length);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                list.Add(asset);
        }

        list.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return list;
    }
}
#endif
