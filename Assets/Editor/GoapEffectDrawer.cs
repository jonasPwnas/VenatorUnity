#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GoapEffect))]
public class GoapEffectDrawer : PropertyDrawer
{
    private const float PADDING = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keyProp = property.FindPropertyRelative("m_key");
        SerializedProperty opProp = property.FindPropertyRelative("m_op");
        SerializedProperty valueProp = property.FindPropertyRelative("m_intValue");

        Rect r = position;
        r.height = EditorGUIUtility.singleLineHeight;

        float wKey = r.width * 0.45f;
        float wOp = r.width * 0.25f;
        float wVal = r.width - wKey - wOp - (PADDING * 2f);

        Rect keyRect = new Rect(r.x, r.y, wKey, r.height);
        Rect opRect = new Rect(r.x + wKey + PADDING, r.y, wOp, r.height);
        Rect valRect = new Rect(r.x + wKey + PADDING + wOp + PADDING, r.y, wVal, r.height);

        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        EditorGUI.PropertyField(opRect, opProp, GUIContent.none);
        EditorGUI.PropertyField(valRect, valueProp, GUIContent.none);
    }
}
#endif
