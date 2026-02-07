#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoapUnitBrain))]
public class GoapUnitBrainEditor : Editor
{
    private GoapUnitTypeDefinition[] m_unitTypes;
    private string[] m_unitTypeNames;

    private void OnEnable()
    {
        string[] guids = AssetDatabase.FindAssets("t:GoapUnitTypeDefinition");
        m_unitTypes = new GoapUnitTypeDefinition[guids.Length];
        m_unitTypeNames = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GoapUnitTypeDefinition asset = AssetDatabase.LoadAssetAtPath<GoapUnitTypeDefinition>(path);

            m_unitTypes[i] = asset;
            m_unitTypeNames[i] = asset != null ? asset.name : "NULL";
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GoapUnitBrain brain = (GoapUnitBrain)target;

        if (m_unitTypes == null || m_unitTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("No GoapUnitTypeDefinition assets found. Create one via Tools/GOAP/GOAP Designer Tool.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Unit Type Dropdown", EditorStyles.boldLabel);

        // Find current selection index (best-effort)
        SerializedProperty unitTypeProp = serializedObject.FindProperty("m_unitType");
        GoapUnitTypeDefinition current = unitTypeProp.objectReferenceValue as GoapUnitTypeDefinition;

        int currentIndex = 0;
        for (int i = 0; i < m_unitTypes.Length; i++)
        {
            if (m_unitTypes[i] == current)
            {
                currentIndex = i;
                break;
            }
        }

        int newIndex = EditorGUILayout.Popup("Unit Type", currentIndex, m_unitTypeNames);

        if (newIndex != currentIndex)
        {
            unitTypeProp.objectReferenceValue = m_unitTypes[newIndex];
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
