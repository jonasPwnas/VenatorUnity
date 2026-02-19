using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveAwayCombatState : FsmCombatState
{
    public override bool CanEnter()
    {
        return fsmOwner.GetDistanceToTarget() < 2.5f;
    }

    public override FsmCombatActionType GetCombatActionType()
    {
        return FsmCombatActionType.MovementAction;
    }
    
    public override FsmEnemyCombat GetStateTag()
    {
        return FsmEnemyCombat.MoveAway;
    }

    public override void EnterState()
    {
        base.EnterState();
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
        if (fsmOwner.GetDistanceToTarget() > 10f)
        {
            print("stopped move away");
            fsmOwner.npcFsmController.StopMoveTo();
            ExitState();
            yield break;
        }

        print("tries move away");
        
        Vector3 moveToRandom = transform.position + transform.forward * Random.Range(-7f, -4f);
        NavMesh.SamplePosition(moveToRandom, out var hit, Random.Range(1f, 4f), NavMesh.AllAreas);
        fsmOwner.npcFsmController.DoMoveTo(hit.position);
        yield return new WaitForSeconds(trackFreq);
        StartCoroutine(DoMoveToTrackedTarget(trackFreq));
    }
}
