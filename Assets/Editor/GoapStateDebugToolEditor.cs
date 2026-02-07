#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoapDebugTester))]
public class GoapStateDebugToolEditor : Editor
{
    // Foldouts
    private bool m_showReferences = true;
    private bool m_showBehaviour = true;
    private bool m_showPerception = true;
    private bool m_showHealth = true;
    private bool m_showCombat = true;
    private bool m_showElements = true;
    private bool m_showStages = true;
    private bool m_showPlan = true;

    // Live UI
    private bool m_liveRepaint = true;

    // Cached props
    private SerializedProperty p_agent;
    private SerializedProperty p_worldStateBuilder;

    private SerializedProperty p_autoRead;
    private SerializedProperty p_autoApply;

    // Perception / Navigation
    private SerializedProperty p_alerted;
    private SerializedProperty p_targetKnown;
    private SerializedProperty p_targetVisible;
    private SerializedProperty p_targetRange;
    private SerializedProperty p_facingTarget;
    private SerializedProperty p_hasPathToTarget;
    private SerializedProperty p_targetAttackingNow;
    private SerializedProperty p_distanceBand;

    // Health
    private SerializedProperty p_selfHealth01;
    private SerializedProperty p_targetHealth01;

    // Combat / Anti-cheese
    private SerializedProperty p_attackCooldownReady;
    private SerializedProperty p_isBlocking;
    private SerializedProperty p_strongPrimed;
    private SerializedProperty p_incomingThreat01;
    private SerializedProperty p_combatMode;
    private SerializedProperty p_pressureState;
    private SerializedProperty p_cheeseScore01;
    private SerializedProperty p_playerInLockout;
    private SerializedProperty p_isLockedOn;

    // Elements / Charging
    private SerializedProperty p_activeElement;
    private SerializedProperty p_fireCharges;
    private SerializedProperty p_waterCharges;
    private SerializedProperty p_windCharges;
    private SerializedProperty p_chargePointKnown;
    private SerializedProperty p_atChargePoint;
    private SerializedProperty p_desiredChargeElement;

    // Patrol / Search
    private SerializedProperty p_patrolStage;
    private SerializedProperty p_searchStage;

    private void OnEnable()
    {
        CacheProps();
    }

    private void CacheProps()
    {
        // References
        p_agent = serializedObject.FindProperty("m_agent");
        p_worldStateBuilder = serializedObject.FindProperty("m_worldStateBuilder");

        // Behaviour
        p_autoRead = serializedObject.FindProperty("m_autoRead");
        p_autoApply = serializedObject.FindProperty("m_autoApply");

        // Perception / Navigation
        p_alerted = serializedObject.FindProperty("m_alerted");
        p_targetKnown = serializedObject.FindProperty("m_targetKnown");
        p_targetVisible = serializedObject.FindProperty("m_targetVisible");
        p_targetRange = serializedObject.FindProperty("m_targetRange");
        p_facingTarget = serializedObject.FindProperty("m_facingTarget");
        p_hasPathToTarget = serializedObject.FindProperty("m_hasPathToTarget");
        p_targetAttackingNow = serializedObject.FindProperty("m_targetAttackingNow");
        p_distanceBand = serializedObject.FindProperty("m_distanceBand");

        // Health
        p_selfHealth01 = serializedObject.FindProperty("m_selfHealth01");
        p_targetHealth01 = serializedObject.FindProperty("m_targetHealth01");

        // Combat / Anti-cheese
        p_attackCooldownReady = serializedObject.FindProperty("m_attackCooldownReady");
        p_isBlocking = serializedObject.FindProperty("m_isBlocking");
        p_strongPrimed = serializedObject.FindProperty("m_strongPrimed");
        p_incomingThreat01 = serializedObject.FindProperty("m_incomingThreat01");
        p_combatMode = serializedObject.FindProperty("m_combatMode");
        p_pressureState = serializedObject.FindProperty("m_pressureState");
        p_cheeseScore01 = serializedObject.FindProperty("m_cheeseScore01");
        p_playerInLockout = serializedObject.FindProperty("m_playerInLockout");
        p_isLockedOn = serializedObject.FindProperty("m_isLockedOn");

        // Elements / Charging
        p_activeElement = serializedObject.FindProperty("m_activeElement");
        p_fireCharges = serializedObject.FindProperty("m_fireCharges");
        p_waterCharges = serializedObject.FindProperty("m_waterCharges");
        p_windCharges = serializedObject.FindProperty("m_windCharges");
        p_chargePointKnown = serializedObject.FindProperty("m_chargePointKnown");
        p_atChargePoint = serializedObject.FindProperty("m_atChargePoint");
        p_desiredChargeElement = serializedObject.FindProperty("m_desiredChargeElement");

        // Patrol / Search
        p_patrolStage = serializedObject.FindProperty("m_patrolStage");
        p_searchStage = serializedObject.FindProperty("m_searchStage");
    }

    public override void OnInspectorGUI()
    {
        var tool = (GoapDebugTester)target;

        serializedObject.Update();

        DrawTopSummary(tool);

        EditorGUILayout.Space(8);

        DrawReferencesSection();
        DrawBehaviourSection();
        DrawQuickControls(tool);

        EditorGUILayout.Space(8);

        DrawPerceptionSection();
        DrawHealthSection();
        DrawCombatSection();
        DrawElementsSection();
        DrawStagesSection();

        DrawPlanDebug(tool);

        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying && m_liveRepaint)
            Repaint();
    }

    private void DrawTopSummary(GoapDebugTester tool)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("GOAP Debug", EditorStyles.boldLabel);

        var agent = tool.Agent;
        if (agent == null)
        {
            EditorGUILayout.HelpBox("No GoapAgent found on this object. Plan debug will be unavailable.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.LabelField("Current Goal", agent.CurrentGoalName);
            EditorGUILayout.LabelField("Current Action", agent.CurrentActionId);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawReferencesSection()
    {
        m_showReferences = EditorGUILayout.Foldout(m_showReferences, "References", true);
        if (!m_showReferences) return;

        EditorGUILayout.BeginVertical("box");
        DrawIfExists(p_agent, "Goap Agent");
        DrawIfExists(p_worldStateBuilder, "World State Builder");
        EditorGUILayout.EndVertical();
    }

    private void DrawBehaviourSection()
    {
        m_showBehaviour = EditorGUILayout.Foldout(m_showBehaviour, "Behaviour", true);
        if (!m_showBehaviour) return;

        EditorGUILayout.BeginVertical("box");
        DrawIfExists(p_autoRead, "Auto Read");
        DrawIfExists(p_autoApply, "Auto Apply");

        if (p_autoApply != null && p_autoApply.boolValue)
        {
            EditorGUILayout.HelpBox(
                "Auto Apply overrides whatever your sensors write into the world state. Use only for testing.",
                MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "Tip: Use Auto Read while playing to watch values update live.",
                MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickControls(GoapDebugTester tool)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Quick Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Read From AI"))
        {
            tool.ReadFromAi();
            EditorUtility.SetDirty(tool);
        }

        if (GUILayout.Button("Apply To AI"))
        {
            tool.ApplyToAi();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawPerceptionSection()
    {
        m_showPerception = EditorGUILayout.Foldout(m_showPerception, "Perception & Navigation", true);
        if (!m_showPerception) return;

        EditorGUILayout.BeginVertical("box");

        DrawIfExists(p_alerted, "Alerted (0/1)");
        DrawIfExists(p_targetKnown, "Target Known (0/1)");
        DrawIfExists(p_targetVisible, "Target Visible (0/1)");
        DrawIfExists(p_targetRange, "Target Range (0 none, 1 far, 2 chase, 3 melee)");
        DrawIfExists(p_facingTarget, "Facing Target (0/1)");
        DrawIfExists(p_hasPathToTarget, "Has Path To Target (0/1)");
        DrawIfExists(p_targetAttackingNow, "Target Attacking Now (0/1)");
        DrawIfExists(p_distanceBand, "Distance Band (your mapping)");

        EditorGUILayout.EndVertical();
    }

    private void DrawHealthSection()
    {
        m_showHealth = EditorGUILayout.Foldout(m_showHealth, "Health", true);
        if (!m_showHealth) return;

        EditorGUILayout.BeginVertical("box");
        DrawIfExists(p_selfHealth01, "Self Health 01 (0..100)");
        DrawIfExists(p_targetHealth01, "Target Health 01 (0..100)");
        EditorGUILayout.EndVertical();
    }

    private void DrawCombatSection()
    {
        m_showCombat = EditorGUILayout.Foldout(m_showCombat, "Combat & Anti-Cheese", true);
        if (!m_showCombat) return;

        EditorGUILayout.BeginVertical("box");

        DrawIfExists(p_attackCooldownReady, "Attack Cooldown Ready (0/1)");
        DrawIfExists(p_isBlocking, "Is Blocking (0/1)");
        DrawIfExists(p_strongPrimed, "Strong Primed (0/1)");
        DrawIfExists(p_incomingThreat01, "Incoming Threat 01 (0..100)");

        DrawIfExists(p_combatMode, "Combat Mode (0 none, 1 aggro, 2 tactical, 3 baiting, 4 elemental)");
        DrawIfExists(p_pressureState, "Pressure State (your mapping)");
        DrawIfExists(p_playerInLockout, "Player In Lockout (0/1)");
        DrawIfExists(p_isLockedOn, "Is Locked On (0/1)");

        EditorGUILayout.Space(4);
        DrawIfExists(p_cheeseScore01, "Cheese Score 01 (0..100)");

        EditorGUILayout.EndVertical();
    }

    private void DrawElementsSection()
    {
        m_showElements = EditorGUILayout.Foldout(m_showElements, "Elements & Charging", true);
        if (!m_showElements) return;

        EditorGUILayout.BeginVertical("box");

        DrawIfExists(p_activeElement, "Active Element (0 none, 1 fire, 2 water, 3 wind/air)");
        DrawIfExists(p_fireCharges, "Fire Charges (0..3)");
        DrawIfExists(p_waterCharges, "Water Charges (0..3)");
        DrawIfExists(p_windCharges, "Wind Charges (0..3)");

        EditorGUILayout.Space(4);

        DrawIfExists(p_chargePointKnown, "Charge Point Known (0/1)");
        DrawIfExists(p_atChargePoint, "At Charge Point (0/1)");
        DrawIfExists(p_desiredChargeElement, "Desired Charge Element (0 none, 1 fire, 2 water, 3 air)");

        EditorGUILayout.EndVertical();
    }

    private void DrawStagesSection()
    {
        m_showStages = EditorGUILayout.Foldout(m_showStages, "Patrol & Search Stages", true);
        if (!m_showStages) return;

        EditorGUILayout.BeginVertical("box");
        DrawIfExists(p_patrolStage, "Patrol Stage");
        DrawIfExists(p_searchStage, "Search Stage");
        EditorGUILayout.EndVertical();
    }

    private void DrawPlanDebug(GoapDebugTester tool)
    {
        EditorGUILayout.Space(8);

        m_showPlan = EditorGUILayout.Foldout(m_showPlan, "GOAP Plan Debug", true);
        if (!m_showPlan)
            return;

        EditorGUILayout.BeginVertical("box");

        m_liveRepaint = EditorGUILayout.Toggle("Live Update", m_liveRepaint);

        GoapAgent agent = tool.Agent;
        if (agent == null)
        {
            EditorGUILayout.HelpBox(
                "No GoapAgent reference found. Add GoapDebugTester to the same object as GoapAgent.",
                MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField("Current Goal", agent.CurrentGoalName);
        EditorGUILayout.LabelField("Current Action", agent.CurrentActionId);
        EditorGUILayout.LabelField("Plan Steps", agent.CurrentPlanCount.ToString());

        string planText = agent.GetPlanDebugString(16);

        EditorGUILayout.Space(4);
        EditorGUILayout.TextArea(planText, GUILayout.MinHeight(120));

        EditorGUILayout.EndVertical();
    }

    private void DrawIfExists(SerializedProperty prop, string label)
    {
        if (prop == null)
        {
            // Helps when scripts are out of sync (old field names, etc.)
            EditorGUILayout.HelpBox($"Missing field on GoapDebugTester: {label}", MessageType.None);
            return;
        }

        EditorGUILayout.PropertyField(prop, new GUIContent(label));
    }
}
#endif
