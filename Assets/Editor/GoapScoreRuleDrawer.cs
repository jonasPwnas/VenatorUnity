#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GoapScoreRule))]
public class GoapScoreRuleDrawer : PropertyDrawer
{
    private const float PADDING = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("m_ruleType");
        GoapScoreRuleType type = (GoapScoreRuleType)typeProp.enumValueIndex;

        if (type == GoapScoreRuleType.AddByCurve01FromInt01)
            return (EditorGUIUtility.singleLineHeight * 2f) + PADDING;

        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty typeProp = property.FindPropertyRelative("m_ruleType");
        GoapScoreRuleType type = (GoapScoreRuleType)typeProp.enumValueIndex;

        Rect line1 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        float wType = line1.width * 0.35f;
        Rect typeRect = new Rect(line1.x, line1.y, wType, line1.height);
        Rect restRect = new Rect(line1.x + wType + PADDING, line1.y, line1.width - wType - PADDING, line1.height);

        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        if (type == GoapScoreRuleType.AddIfConditionTrue || type == GoapScoreRuleType.AddIfConditionFalse)
        {
            SerializedProperty condProp = property.FindPropertyRelative("m_condition");
            SerializedProperty addProp = property.FindPropertyRelative("m_addScore");

            float wCond = restRect.width * 0.72f;
            Rect condRect = new Rect(restRect.x, restRect.y, wCond, restRect.height);
            Rect addRect = new Rect(restRect.x + wCond + PADDING, restRect.y, restRect.width - wCond - PADDING, restRect.height);

            EditorGUI.PropertyField(condRect, condProp, GUIContent.none);
            EditorGUI.PropertyField(addRect, addProp, GUIContent.none);
            return;
        }

        if (type == GoapScoreRuleType.AddByCurve01FromInt01)
        {
            SerializedProperty keyProp = property.FindPropertyRelative("m_sourceKey");
            SerializedProperty curveProp = property.FindPropertyRelative("m_curve");
            SerializedProperty multProp = property.FindPropertyRelative("m_curveMultiplier");

            float wKey = restRect.width * 0.45f;
            Rect keyRect = new Rect(restRect.x, restRect.y, wKey, restRect.height);
            Rect multRect = new Rect(restRect.x + wKey + PADDING, restRect.y, restRect.width - wKey - PADDING, restRect.height);

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(multRect, multProp, GUIContent.none);

            Rect line2 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + PADDING, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(line2, curveProp, GUIContent.none);
        }
    }
}
#endif
