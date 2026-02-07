using System.Collections.Generic;
using UnityEngine;

public static class GoapPlanner
{
    private class Node
    {
        public GoapWorldState State;
        public Node Parent;
        public GoapActionDefinition Action;
        public int CostSoFar;
    }

    private const int MAX_NODES = 2000;

    public static bool TryPlan(
        GoapWorldState startState,
        List<GoapActionDefinition> actions,
        GoapGoalDefinition goal,
        System.Func<GoapActionDefinition, int> getActionCost,
        out List<GoapActionDefinition> plan)
    {
        plan = null;

        if (goal == null)
            return false;

        Node start = new Node
        {
            State = startState.Clone(),
            Parent = null,
            Action = null,
            CostSoFar = 0
        };

        List<Node> open = new List<Node>(64) { start };
        HashSet<ulong> closed = new HashSet<ulong>();

        int nodeCount = 0;

        while (open.Count > 0 && nodeCount < MAX_NODES)
        {
            nodeCount++;

            Node current = PopLowestCost(open);
            ulong hash = current.State.GetHash64();

            if (closed.Contains(hash))
                continue;

            closed.Add(hash);

            if (goal.IsSatisfied(current.State))
            {
                plan = ReconstructPlan(current);
                return true;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                GoapActionDefinition action = actions[i];

                if (!ActionPreconditionsMet(action, current.State))
                    continue;

                GoapWorldState next = current.State.Clone();
                ApplyEffects(action, next);

                Node nextNode = new Node
                {
                    State = next,
                    Parent = current,
                    Action = action,
                    CostSoFar = current.CostSoFar + Mathf.Max(1, getActionCost(action))
                };

                open.Add(nextNode);
            }
        }

        return false;
    }

    private static Node PopLowestCost(List<Node> open)
    {
        int bestIndex = 0;
        int bestCost = open[0].CostSoFar;

        for (int i = 1; i < open.Count; i++)
        {
            if (open[i].CostSoFar < bestCost)
            {
                bestCost = open[i].CostSoFar;
                bestIndex = i;
            }
        }

        Node node = open[bestIndex];
        open.RemoveAt(bestIndex);
        return node;
    }

    private static bool ActionPreconditionsMet(GoapActionDefinition action, GoapWorldState worldState)
    {
        GoapCondition[] conditions = action.Preconditions;

        if (conditions == null)
            return true;

        for (int i = 0; i < conditions.Length; i++)
        {
            if (!conditions[i].Evaluate(worldState))
                return false;
        }

        return true;
    }

    private static void ApplyEffects(GoapActionDefinition action, GoapWorldState worldState)
    {
        GoapEffect[] effects = action.Effects;

        if (effects == null)
            return;

        for (int i = 0; i < effects.Length; i++)
            effects[i].Apply(worldState);
    }

    private static List<GoapActionDefinition> ReconstructPlan(Node node)
    {
        List<GoapActionDefinition> list = new List<GoapActionDefinition>();

        Node current = node;
        while (current != null && current.Action != null)
        {
            list.Add(current.Action);
            current = current.Parent;
        }

        list.Reverse();
        return list;
    }
}
