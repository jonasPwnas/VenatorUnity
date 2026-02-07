using UnityEngine;
using UnityEngine.AI;

public class Walk : MonoBehaviour, IGoapActionListener
{
    private Vector3 target;
    private NavMeshAgent agent;
    public GameObject player;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

    }
    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        agent.SetDestination(target);
    }

    public void OnGoapActionEvent(GoapAgent agent, string actionId, GoapActionPhase phase)
    {
       if(phase == GoapActionPhase.Started && actionId == "ChaseTarget")
        {
            SetTarget(player.transform.position);
        }
    }
}
