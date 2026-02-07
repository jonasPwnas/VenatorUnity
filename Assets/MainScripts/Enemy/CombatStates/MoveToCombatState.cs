using System.Collections;
using UnityEngine;

public class MoveToCombatState : FsmCombatState
{
    
    public override FsmCombatActionType GetCombatActionType()
    {
        return FsmCombatActionType.MovementAction;
    }
    
    public override FsmEnemyCombat GetStateTag()
    {
        return FsmEnemyCombat.MoveTo;
    }

    public override void EnterState()
    {
        StartCoroutine(DoMoveToTrackedTarget(0.5f));
    }

    public override void ExitState()
    {
        StopAllCoroutines();
        base.ExitState();
    }

    public override void AbortState(bool doCooldown)
    {
        StopAllCoroutines();
        fsmOwner.npcFsmController.StopMoveTo();
        base.AbortState(doCooldown);
    }

    IEnumerator DoMoveToTrackedTarget(float trackFreq)
    {
        if (fsmOwner.GetDistanceToTarget() < 2f)
        {
            fsmOwner.npcFsmController.StopMoveTo();
            ExitState();
            yield break;
        }
        
        fsmOwner.npcFsmController.DoMoveTo(fsmOwner.playerController.transform.position);
        yield return new WaitForSeconds(trackFreq);
        StartCoroutine(DoMoveToTrackedTarget(trackFreq));
    }
}
