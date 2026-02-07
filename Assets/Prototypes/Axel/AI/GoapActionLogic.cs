using UnityEngine;

public abstract class GoapActionLogic : ScriptableObject
{
    public abstract bool CanRun(GoapAgent agent);
    public abstract void Begin(GoapAgent agent);
    public abstract bool Tick(GoapAgent agent, float dt);
    public abstract void End(GoapAgent agent, bool success);
    public virtual bool HasFinishedSuccessfully(GoapAgent agent)
    {
        return true;
    }
}
