using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class GoapAgent : MonoBehaviour
{
    public event Action<GoapAgent, string, GoapActionPhase> OnActionEvent;

    [SerializeField] private bool m_isEnabled = true;

    private GoapUnitTypeDefinition m_unitType;
    private GoapWorldStateBuilder m_worldStateBuilder;
    private NavMeshAgent m_navMeshAgent;

    private GoapGoalDefinition m_currentGoal;
    private List<GoapActionDefinition> m_availableActions;
    private List<GoapActionDefinition> m_currentPlan;
    private int m_currentPlanIndex;

    private GoapActionDefinition m_runningAction;
    private GoapActionLogic m_runningLogic;
    private GoapActionRuntimeMemory m_actionMemory;

    private Dictionary<GoapGoalDefinition, float> m_goalLastCompletedTime;
    private Dictionary<string, bool> m_actionSignals;
    private IGoapActionListener[] m_actionListeners;

    private bool m_isForceReplaning;

    public GoapWorldStateBuilder WorldStateBuilder => m_worldStateBuilder;
    public NavMeshAgent NavMeshAgent => m_navMeshAgent;
    public GoapActionRuntimeMemory ActionMemory => m_actionMemory;

    private void Awake()
    {
        m_goalLastCompletedTime = new Dictionary<GoapGoalDefinition, float>(16);
        m_actionSignals = new Dictionary<string, bool>(32);

        m_worldStateBuilder = GetComponent<GoapWorldStateBuilder>();
        m_navMeshAgent = GetComponent<NavMeshAgent>();

        m_actionListeners = GetComponents<IGoapActionListener>();

        m_actionMemory = GetComponent<GoapActionRuntimeMemory>();
        if (m_actionMemory == null)
            m_actionMemory = gameObject.AddComponent<GoapActionRuntimeMemory>();
    }

    private void Update()
    {
        if (!m_isEnabled)
            return;

        if (m_unitType == null || m_worldStateBuilder == null)
            return;

        TickGoalsAndPlanning();
        TickActionExecution(Time.deltaTime);
    }

    public void SetUnitType(GoapUnitTypeDefinition unitType)
    {
        m_unitType = unitType;
        BuildAvailableActions();
        ForceReplan();
    }

    public void SetSignal(string signalId)
    {
        if (string.IsNullOrEmpty(signalId))
            return;

        m_actionSignals[signalId] = true;
    }

    public bool ConsumeSignal(string signalId)
    {
        if (string.IsNullOrEmpty(signalId))
            return false;

        if (!m_actionSignals.TryGetValue(signalId, out bool value))
            return false;

        if (!value)
            return false;

        m_actionSignals[signalId] = false;
        return true;
    }

    public void RaiseActionEvent(string actionId, GoapActionPhase phase)
    {
        OnActionEvent?.Invoke(this, actionId, phase);

        if (m_actionListeners != null)
        {
            for (int i = 0; i < m_actionListeners.Length; i++)
                m_actionListeners[i].OnGoapActionEvent(this, actionId, phase);
        }
    }

    private void BuildAvailableActions()
    {
        m_availableActions = new List<GoapActionDefinition>(32);

        if (m_unitType == null || m_unitType.Actions == null)
            return;

        for (int i = 0; i < m_unitType.Actions.Count; i++)
        {
            GoapUnitTypeDefinition.ActionEntry entry = m_unitType.Actions[i];

            if (!entry.Enabled || entry.Action == null)
                continue;

            m_availableActions.Add(entry.Action);
        }
    }

    private void TickGoalsAndPlanning()
    {
        GoapWorldState worldState = m_worldStateBuilder.WorldState;
        float timeNow = Time.time;

        GoapGoalDefinition bestGoal = null;
        int bestScore = int.MinValue;

        if (m_unitType != null && m_unitType.Goals != null)
        {
            for (int i = 0; i < m_unitType.Goals.Count; i++)
            {
                GoapUnitTypeDefinition.GoalEntry entry = m_unitType.Goals[i];
                GoapGoalDefinition goal = entry.Goal;

                if (!entry.Enabled || goal == null)
                    continue;

                float lastCompleted = GetGoalLastCompletedTime(goal);

                if (!goal.IsEligible(worldState, timeNow, lastCompleted))
                    continue;

                bool isCurrentGoal = goal == m_currentGoal;

                int score = goal.ComputeScore(worldState, isCurrentGoal);

                if (entry.BasePriorityOverride >= 0)
                    score += (entry.BasePriorityOverride - goal.BasePriority);

                score = Mathf.RoundToInt(score * Mathf.Max(0f, entry.ScoreMultiplier));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestGoal = goal;
                }
            }
        }

        if (bestGoal == null)
            return;

        if (m_currentGoal == null)
        {
            m_currentGoal = bestGoal;
            ForceReplan();
            return;
        }

        if (bestGoal == m_currentGoal)
            return;

        int currentScore = m_currentGoal.ComputeScore(worldState, true);
        int requiredLead = m_currentGoal.Hysteresis;

        if (bestScore >= currentScore + requiredLead)
        {
            m_currentGoal = bestGoal;
            ForceReplan();
        }
    }

    private void TickActionExecution(float dt)
    {
        if (m_currentGoal == null)
            return;

        if (m_currentGoal.IsSatisfied(m_worldStateBuilder.WorldState))
        {
            ApplyGoalCompletionEffectsToLiveWorld(m_currentGoal);
            MarkGoalCompleted(m_currentGoal);

            m_currentGoal = null;
            StopRunningAction(false, false, true);

            m_currentPlan = null;
            return;
        }

        if (m_runningAction == null)
        {
            if (m_currentPlan == null || m_currentPlanIndex >= m_currentPlan.Count)
            {
                TryBuildPlan();
                return;
            }

            StartNextAction();
            return;
        }

        if (m_runningLogic == null)
        {
            StopRunningAction(false, false, true);
            return;
        }

        bool done = m_runningLogic.Tick(this, dt);
        if (!done)
            return;

        bool success = m_runningLogic.HasFinishedSuccessfully(this);
        StopRunningAction(success, !success);
    }

    private void StartNextAction()
    {
        m_runningAction = m_currentPlan[m_currentPlanIndex];
        m_currentPlanIndex++;

        // ✅ CRITICAL FIX: Re-check Preconditions against LIVE world state
        if (!AreActionPreconditionsMetNow(m_runningAction, m_worldStateBuilder.WorldState))
        {
            StopRunningAction(false, false, true);
            ForceReplan();
            return;
        }

        m_runningLogic = m_runningAction.Logic;

        if (m_runningLogic == null || !m_runningLogic.CanRun(this))
        {
            StopRunningAction(false, false, true);
            ForceReplan();
            return;
        }

        RaiseActionEvent(m_runningAction.ActionId, GoapActionPhase.Started);
        m_runningLogic.Begin(this);
    }

    private bool AreActionPreconditionsMetNow(GoapActionDefinition action, GoapWorldState liveState)
    {
        if (action == null)
            return false;

        var conds = action.Preconditions;
        if (conds == null)
            return true;

        for (int i = 0; i < conds.Length; i++)
        {
            if (!conds[i].Evaluate(liveState))
                return false;
        }

        return true;
    }

    private void StopRunningAction(bool success, bool requestReplanOnFail, bool aborted = false)
    {
        if (m_runningLogic != null)
            m_runningLogic.End(this, success);

        if (success && m_runningAction != null)
            ApplyActionEffectsToLiveWorld(m_runningAction);

        if (m_runningAction != null)
        {
            GoapActionPhase phase = aborted ? GoapActionPhase.Aborted :
                (success ? GoapActionPhase.Completed : GoapActionPhase.Failed);

            RaiseActionEvent(m_runningAction.ActionId, phase);
        }

        m_runningAction = null;
        m_runningLogic = null;

        if (!success && requestReplanOnFail)
            ForceReplan();
    }

    private void TryBuildPlan()
    {
        if (m_availableActions == null || m_availableActions.Count == 0)
            return;

        if (m_currentGoal == null)
            return;

        bool planned = GoapPlanner.TryPlan(
            m_worldStateBuilder.WorldState,
            m_availableActions,
            m_currentGoal,
            GetActionCost,
            out List<GoapActionDefinition> plan);

        if (!planned || plan == null || plan.Count == 0)
        {
            m_currentPlan = null;
            return;
        }

        m_currentPlan = plan;
        m_currentPlanIndex = 0;
    }

    private int GetActionCost(GoapActionDefinition action)
    {
        if (m_unitType == null || m_unitType.Actions == null)
            return action.BaseCost;

        for (int i = 0; i < m_unitType.Actions.Count; i++)
        {
            GoapUnitTypeDefinition.ActionEntry entry = m_unitType.Actions[i];

            if (entry.Action != action)
                continue;

            if (entry.CostOverride >= 0)
                return entry.CostOverride;

            break;
        }

        return action.BaseCost;
    }

    private float GetGoalLastCompletedTime(GoapGoalDefinition goal)
    {
        if (m_goalLastCompletedTime.TryGetValue(goal, out float t))
            return t;

        return -9999f;
    }

    private void MarkGoalCompleted(GoapGoalDefinition goal)
    {
        m_goalLastCompletedTime[goal] = Time.time;
    }

    private void ForceReplan()
    {
        if (m_isForceReplaning)
            return;

        m_isForceReplaning = true;
        StopRunningAction(false, false);
        m_currentPlan = null;
        m_currentPlanIndex = 0;
        m_isForceReplaning = false;
    }

    // Debug helpers
    public string CurrentGoalName => m_currentGoal != null ? m_currentGoal.name : "None";
    public string CurrentActionId => m_runningAction != null ? m_runningAction.ActionId : "None";
    public int CurrentPlanStepIndex => Mathf.Clamp(m_currentPlanIndex - 1, -1, 999999);
    public int CurrentPlanCount => m_currentPlan != null ? m_currentPlan.Count : 0;
    public bool HasCurrentPlan => m_currentPlan != null && m_currentPlan.Count > 0;
    public IReadOnlyList<GoapActionDefinition> CurrentPlan => m_currentPlan;

    public string GetPlanDebugString(int maxSteps = 12)
    {
        if (m_currentGoal == null)
            return "Goal: None\nPlan: None";

        if (m_currentPlan == null || m_currentPlan.Count == 0)
            return $"Goal: {m_currentGoal.name}\nPlan: None";

        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        sb.Append("Goal: ").Append(m_currentGoal.name).Append('\n');

        int currentStep = CurrentPlanStepIndex;

        sb.Append("Plan:\n");

        int count = Mathf.Min(m_currentPlan.Count, Mathf.Max(1, maxSteps));
        for (int i = 0; i < count; i++)
        {
            string prefix = (i == currentStep) ? " > " : "   ";
            sb.Append(prefix).Append(i).Append(": ").Append(m_currentPlan[i].ActionId).Append('\n');
        }

        if (m_currentPlan.Count > count)
            sb.Append("   ...\n");

        sb.Append("Running: ").Append(CurrentActionId);

        return sb.ToString();
    }

    public void RequestReplan() => ForceReplan();

    private void ApplyActionEffectsToLiveWorld(GoapActionDefinition action)
    {
        if (action == null || m_worldStateBuilder == null)
            return;

        GoapEffect[] effects = action.Effects;
        if (effects == null || effects.Length == 0)
            return;

        GoapWorldState liveState = m_worldStateBuilder.WorldState;

        for (int i = 0; i < effects.Length; i++)
            effects[i].Apply(liveState);
    }

    private void ApplyGoalCompletionEffectsToLiveWorld(GoapGoalDefinition goal)
    {
        if (goal == null || m_worldStateBuilder == null)
            return;

        GoapEffect[] effects = goal.OnCompletedEffects;

        if (effects == null || effects.Length == 0)
            return;

        GoapWorldState liveState = m_worldStateBuilder.WorldState;

        for (int i = 0; i < effects.Length; i++)
            effects[i].Apply(liveState);
    }
}
