using UnityEngine;

public class IdleCombatState : FsmCombatState
{
    public override FsmCombatActionType GetCombatActionType()
    {
        return FsmCombatActionType.IdleAction;
    }
    
    public override FsmEnemyCombat GetStateTag()
    {
        return FsmEnemyCombat.CombatIdle;
    }
    
    public override void EnterState()
    {
        fsmOwner.npcFsmController.DoIdle();
    }
}
