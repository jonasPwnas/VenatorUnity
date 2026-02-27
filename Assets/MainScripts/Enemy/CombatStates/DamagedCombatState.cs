using UnityEngine;

public class DamagedCombatState : FsmCombatState
{
    public override FsmCombatActionType GetCombatActionType()
    {
        return FsmCombatActionType.DamagedAction;
    }
    
    public override FsmEnemyCombat GetStateTag()
    {
        return FsmEnemyCombat.KnockedDown;
    }

    public override bool CanEnter()
    {
        return false;
    }

    public override void EnterState()
    {
        base.EnterState();
        print("current state: entered damaged state");
    }

    public override bool UseUpdate()
    {
        return true;
    }

    public override void UpdateState()
    {
        print("updating damaged state");
        
        if (fsmOwner.animator.GetNextAnimatorStateInfo(0).IsName("MovementBLT"))
        {
            ExitState();
            print("exited damaged state");
        }
    }
}
