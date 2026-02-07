#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class GoapAuthoringToolWindow : EditorWindow
{
    private enum Tab { Actions, Goals, UnitTypes }

    [SerializeField] private Tab m_tab = Tab.Actions;

    // Scroll positions (one per tab)
    [SerializeField] private Vector2 m_actionsScroll;
    [SerializeField] private Vector2 m_goalsScroll;
    [SerializeField] private Vector2 m_unitTypesScroll;

    private GoapActionDefinition m_selectedAction;
    private GoapGoalDefinition m_selectedGoal;
    private GoapUnitTypeDefinition m_selectedUnitType;

    private SerializedObject m_actionSO;
    private SerializedObject m_goalSO;
    private SerializedObject m_unitTypeSO;

    private ReorderableList m_actionPreconditionsList;
    private ReorderableList m_actionEffectsList;

    private ReorderableList m_goalRequiredList;
    private ReorderableList m_goalDesiredList;
    private ReorderableList m_goalScoreRulesList;
    private ReorderableList m_goalOnCompletedEffectsList;

    private ReorderableList m_unitGoalsList;
    private ReorderableList m_unitActionsList;

    [MenuItem("Tools/GOAP/GOAP Authoring Tool(without 2fA)")]
    public static void Open()
    {
        GetWindow<GoapAuthoringToolWindow>("GOAP Tool");
    }

    private void OnEnable()
    {
        GoapAssetCache.Refresh();
    }

    private void OnGUI()
    {
        DrawTopBar();

        EditorGUILayout.Space(6);

        switch (m_tab)
        {
            case Tab.Actions:
                m_actionsScroll = EditorGUILayout.BeginScrollView(m_actionsScroll);
                DrawActionsTab();
                EditorGUILayout.EndScrollView();
                break;

            case Tab.Goals:
                m_goalsScroll = EditorGUILayout.BeginScrollView(m_goalsScroll);
                DrawGoalsTab();
                EditorGUILayout.EndScrollView();
                break;

            case Tab.UnitTypes:
                m_unitTypesScroll = EditorGUILayout.BeginScrollView(m_unitTypesScroll);
                DrawUnitTypesTab();
                EditorGUILayout.EndScrollView();
                break;
        }
    }

    private void DrawTopBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        m_tab = (Tab)GUILayout.Toolbar((int)m_tab, new[] { "Actions", "Goals", "Unit Types" }, EditorStyles.toolbarButton);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
            GoapAssetCache.Refresh();

        EditorGUILayout.EndHorizontal();
    }

    // ACTIONS

    private void DrawActionsTab()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Action Wizard", EditorStyles.boldLabel);

        DrawAssetPickerRow(
            "Selected Action",
            ref m_selectedAction,
            GoapAssetCache.Actions,
            () => CreateAsset<GoapActionDefinition>("NewGoapAction.asset"),
            onSelectedChanged: SetupSelectedAction);

        EditorGUILayout.EndVertical();

        if (m_selectedAction == null)
        {
            EditorGUILayout.HelpBox("Create or select an Action to edit it here.", MessageType.Info);
            return;
        }

        if (m_actionSO == null)
            SetupSelectedAction();

        m_actionSO.Update();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);

        SerializedProperty idProp = m_actionSO.FindProperty("m_actionId");
        SerializedProperty costProp = m_actionSO.FindProperty("m_baseCost");
        SerializedProperty logicProp = m_actionSO.FindProperty("m_logic");

        EditorGUILayout.PropertyField(idProp, new GUIContent("Action Id"));
        EditorGUILayout.PropertyField(costProp, new GUIContent("Base Cost"));
        EditorGUILayout.PropertyField(logicProp, new GUIContent("Logic"));

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Preconditions", EditorStyles.boldLabel);
        m_actionPreconditionsList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
        m_actionEffectsList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        ApplyAndSave(m_actionSO);
    }

    private void SetupSelectedAction()
    {
        if (m_selectedAction == null)
        {
            m_actionSO = null;
            m_actionPreconditionsList = null;
            m_actionEffectsList = null;
            return;
        }

        m_actionSO = new SerializedObject(m_selectedAction);

        SerializedProperty preProp = m_actionSO.FindProperty("m_preconditions");
        SerializedProperty effProp = m_actionSO.FindProperty("m_effects");

        m_actionPreconditionsList = CreateSimpleList(m_actionSO, preProp, "Preconditions");
        m_actionEffectsList = CreateSimpleList(m_actionSO, effProp, "Effects");
    }

    // GOALS

    private void DrawGoalsTab()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Goal Wizard", EditorStyles.boldLabel);

        DrawAssetPickerRow(
            "Selected Goal",
            ref m_selectedGoal,
            GoapAssetCache.Goals,
            () => CreateAsset<GoapGoalDefinition>("NewGoapGoal.asset"),
            onSelectedChanged: SetupSelectedGoal);

        EditorGUILayout.EndVertical();

        if (m_selectedGoal == null)
        {
            EditorGUILayout.HelpBox("Create or select a Goal to edit it here.", MessageType.Info);
            return;
        }

        if (m_goalSO == null)
            SetupSelectedGoal();

        m_goalSO.Update();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Priority & Stability", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_goalSO.FindProperty("m_basePriority"), new GUIContent("Base Priority"));
        EditorGUILayout.PropertyField(m_goalSO.FindProperty("m_stickinessBonus"), new GUIContent("Stickiness Bonus"));
        EditorGUILayout.PropertyField(m_goalSO.FindProperty("m_hysteresis"), new GUIContent("Hysteresis"));
        EditorGUILayout.PropertyField(m_goalSO.FindProperty("m_cooldownSeconds"), new GUIContent("Cooldown Seconds"));

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Eligibility Gate (Required)", EditorStyles.boldLabel);
        m_goalRequiredList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Satisfied When (Desired)", EditorStyles.boldLabel);
        m_goalDesiredList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Completion Effects (Applied when goal completes)", EditorStyles.boldLabel);
        m_goalOnCompletedEffectsList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Dynamic Score Rules", EditorStyles.boldLabel);
        m_goalScoreRulesList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        ApplyAndSave(m_goalSO);
    }

    private void SetupSelectedGoal()
    {
        if (m_selectedGoal == null)
        {
            m_goalSO = null;
            m_goalRequiredList = null;
            m_goalDesiredList = null;
            m_goalScoreRulesList = null;
            m_goalOnCompletedEffectsList = null;
            return;
        }

        m_goalSO = new SerializedObject(m_selectedGoal);

        m_goalRequiredList = CreateSimpleList(m_goalSO, m_goalSO.FindProperty("m_required"), "Required");
        m_goalDesiredList = CreateSimpleList(m_goalSO, m_goalSO.FindProperty("m_desired"), "Desired");
        m_goalScoreRulesList = CreateSimpleList(m_goalSO, m_goalSO.FindProperty("m_scoreRules"), "Score Rules");
        m_goalOnCompletedEffectsList = CreateSimpleList(m_goalSO, m_goalSO.FindProperty("m_onCompletedEffects"), "On Completed Effects");
    }

    // UNIT TYPES

    private void DrawUnitTypesTab()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Unit Type Builder", EditorStyles.boldLabel);

        DrawAssetPickerRow(
            "Selected Unit Type",
            ref m_selectedUnitType,
            GoapAssetCache.UnitTypes,
            () => CreateAsset<GoapUnitTypeDefinition>("NewGoapUnitType.asset"),
            onSelectedChanged: SetupSelectedUnitType);

        EditorGUILayout.EndVertical();

        if (m_selectedUnitType == null)
        {
            EditorGUILayout.HelpBox("Create or select a Unit Type. Then assign Goals/Actions with dropdown lists below.", MessageType.Info);
            return;
        }

        if (m_unitTypeSO == null)
            SetupSelectedUnitType();

        m_unitTypeSO.Update();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Goals", EditorStyles.boldLabel);
        m_unitGoalsList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
        m_unitActionsList?.DoLayoutList();
        EditorGUILayout.EndVertical();

        ApplyAndSave(m_unitTypeSO);
    }

    private void SetupSelectedUnitType()
    {
        if (m_selectedUnitType == null)
        {
            m_unitTypeSO = null;
            m_unitGoalsList = null;
            m_unitActionsList = null;
            return;
        }

        m_unitTypeSO = new SerializedObject(m_selectedUnitType);

        SerializedProperty goalsProp = m_unitTypeSO.FindProperty("m_goals");
        SerializedProperty actionsProp = m_unitTypeSO.FindProperty("m_actions");

        m_unitGoalsList = new ReorderableList(m_unitTypeSO, goalsProp, true, true, true, true);
        m_unitGoalsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Unit Goals");
        m_unitGoalsList.elementHeight = EditorGUIUtility.singleLineHeight * 2f + 6f;
        m_unitGoalsList.onAddCallback = list =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            SerializedProperty elem = list.serializedProperty.GetArrayElementAtIndex(index);

            elem.FindPropertyRelative("m_enabled").boolValue = true;
            elem.FindPropertyRelative("m_basePriorityOverride").intValue = -1;
            elem.FindPropertyRelative("m_scoreMultiplier").floatValue = 1f;

            List<GoapGoalDefinition> allGoals = GoapAssetCache.Goals;
            if (allGoals.Count > 0)
                elem.FindPropertyRelative("m_goal").objectReferenceValue = allGoals[0];
        };
        m_unitGoalsList.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty elem = goalsProp.GetArrayElementAtIndex(index);

            Rect line1 = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            Rect line2 = new Rect(rect.x, rect.y + 2f + EditorGUIUtility.singleLineHeight + 2f, rect.width, EditorGUIUtility.singleLineHeight);

            DrawAssetDropdown(
                line1,
                "Goal",
                elem.FindPropertyRelative("m_goal"),
                GoapAssetCache.Goals);

            DrawInlineFields(
                line2,
                elem.FindPropertyRelative("m_enabled"),
                elem.FindPropertyRelative("m_basePriorityOverride"),
                elem.FindPropertyRelative("m_scoreMultiplier"),
                "Enabled",
                "Priority Override",
                "Score Mult");
        };

        m_unitActionsList = new ReorderableList(m_unitTypeSO, actionsProp, true, true, true, true);
        m_unitActionsList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Unit Actions");
        m_unitActionsList.elementHeight = EditorGUIUtility.singleLineHeight * 2f + 6f;
        m_unitActionsList.onAddCallback = list =>
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            SerializedProperty elem = list.serializedProperty.GetArrayElementAtIndex(index);

            elem.FindPropertyRelative("m_enabled").boolValue = true;
            elem.FindPropertyRelative("m_costOverride").intValue = -1;

            List<GoapActionDefinition> allActions = GoapAssetCache.Actions;
            if (allActions.Count > 0)
                elem.FindPropertyRelative("m_action").objectReferenceValue = allActions[0];
        };
        m_unitActionsList.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty elem = actionsProp.GetArrayElementAtIndex(index);

            Rect line1 = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            Rect line2 = new Rect(rect.x, rect.y + 2f + EditorGUIUtility.singleLineHeight + 2f, rect.width, EditorGUIUtility.singleLineHeight);

            DrawAssetDropdown(
                line1,
                "Action",
                elem.FindPropertyRelative("m_action"),
                GoapAssetCache.Actions);

            DrawInlineFields(
                line2,
                elem.FindPropertyRelative("m_enabled"),
                elem.FindPropertyRelative("m_costOverride"),
                null,
                "Enabled",
                "Cost Override",
                null);
        };
    }

    // UI

    private void DrawAssetPickerRow<T>(
        string label,
        ref T selected,
        List<T> allAssets,
        System.Func<T> createNew,
        System.Action onSelectedChanged) where T : ScriptableObject
    {
        EditorGUILayout.BeginHorizontal();

        selected = (T)EditorGUILayout.ObjectField(label, selected, typeof(T), false);

        if (GUILayout.Button("Create", GUILayout.Width(70)))
        {
            selected = createNew?.Invoke();
            GoapAssetCache.Refresh();
            onSelectedChanged?.Invoke();
        }

        if (GUILayout.Button("Ping", GUILayout.Width(50)) && selected != null)
            EditorGUIUtility.PingObject(selected);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Quick Pick", GUILayout.Width(70));

        string[] names = BuildNameList(allAssets);
        int currentIndex = IndexOf(allAssets, selected);
        int newIndex = EditorGUILayout.Popup(currentIndex, names);

        if (newIndex != currentIndex && newIndex >= 0 && newIndex < allAssets.Count)
        {
            selected = allAssets[newIndex];
            onSelectedChanged?.Invoke();
        }

        EditorGUILayout.EndHorizontal();
    }

    private ReorderableList CreateSimpleList(SerializedObject so, SerializedProperty arrayProp, string header)
    {
        ReorderableList list = new ReorderableList(so, arrayProp, true, true, true, true);
        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, header);
        list.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
        list.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty elem = arrayProp.GetArrayElementAtIndex(index);
            Rect r = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(r, elem, GUIContent.none);
        };
        return list;
    }

    private void DrawAssetDropdown<T>(Rect rect, string label, SerializedProperty objectRefProp, List<T> allAssets) where T : ScriptableObject
    {
        string[] names = BuildNameList(allAssets);

        T current = objectRefProp.objectReferenceValue as T;
        int currentIndex = IndexOf(allAssets, current);

        int newIndex = EditorGUI.Popup(rect, label, currentIndex, names);

        if (newIndex >= 0 && newIndex < allAssets.Count)
            objectRefProp.objectReferenceValue = allAssets[newIndex];
    }

    private void DrawInlineFields(
        Rect rect,
        SerializedProperty enabledProp,
        SerializedProperty intProp,
        SerializedProperty floatProp,
        string enabledLabel,
        string intLabel,
        string floatLabel)
    {
        float pad = 6f;
        float w1 = rect.width * 0.25f;
        float w2 = rect.width * 0.35f;
        float w3 = rect.width - w1 - w2 - (pad * 2f);

        Rect r1 = new Rect(rect.x, rect.y, w1, rect.height);
        Rect r2 = new Rect(rect.x + w1 + pad, rect.y, w2, rect.height);
        Rect r3 = new Rect(rect.x + w1 + pad + w2 + pad, rect.y, w3, rect.height);

        if (enabledProp != null)
            enabledProp.boolValue = EditorGUI.ToggleLeft(r1, enabledLabel, enabledProp.boolValue);

        if (intProp != null && !string.IsNullOrEmpty(intLabel))
            EditorGUI.PropertyField(r2, intProp, new GUIContent(intLabel));

        if (floatProp != null && !string.IsNullOrEmpty(floatLabel))
            EditorGUI.PropertyField(r3, floatProp, new GUIContent(floatLabel));
    }

    private void ApplyAndSave(SerializedObject so)
    {
        if (so == null)
            return;

        if (so.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(so.targetObject);
            AssetDatabase.SaveAssets();
        }
    }

    private T CreateAsset<T>(string defaultName) where T : ScriptableObject
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create " + typeof(T).Name,
            defaultName,
            "asset",
            "Choose where to save the asset.");

        if (string.IsNullOrEmpty(path))
            return null;

        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        Selection.activeObject = asset;
        return asset;
    }

    private string[] BuildNameList<T>(List<T> list) where T : ScriptableObject
    {
        if (list == null || list.Count == 0)
            return new[] { "None" };

        string[] names = new string[list.Count];
        for (int i = 0; i < list.Count; i++)
            names[i] = list[i] != null ? list[i].name : "NULL";

        return names;
    }

    private int IndexOf<T>(List<T> list, T item) where T : ScriptableObject
    {
        if (list == null || list.Count == 0)
            return 0;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == item)
                return i;
        }

        return 0;
    }
}
#endif
