using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/GOAP/Unit Type Definition")]
public class GoapUnitTypeDefinition : ScriptableObject
{
    [Serializable]
    public class GoalEntry
    {
        [SerializeField] private GoapGoalDefinition m_goal;
        [SerializeField] private bool m_enabled = true;

        [Tooltip("-1 = use goal base priority")]
        [SerializeField] private int m_basePriorityOverride = -1;

        [Tooltip("Multiplies the final computed score (personality knob).")]
        [SerializeField] private float m_scoreMultiplier = 1f;

        public GoapGoalDefinition Goal => m_goal;
        public bool Enabled => m_enabled;
        public int BasePriorityOverride => m_basePriorityOverride;
        public float ScoreMultiplier => m_scoreMultiplier;
    }

    [Serializable]
    public class ActionEntry
    {
        [SerializeField] private GoapActionDefinition m_action;
        [SerializeField] private bool m_enabled = true;

        [Tooltip("-1 = use action base cost")]
        [SerializeField] private int m_costOverride = -1;

        public GoapActionDefinition Action => m_action;
        public bool Enabled => m_enabled;
        public int CostOverride => m_costOverride;
    }

    [Header("Designer Lists")]
    [SerializeField] private List<GoalEntry> m_goals = new List<GoalEntry>();
    [SerializeField] private List<ActionEntry> m_actions = new List<ActionEntry>();

    public List<GoalEntry> Goals => m_goals;
    public List<ActionEntry> Actions => m_actions;
}
