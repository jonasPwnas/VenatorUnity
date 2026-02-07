using System;
using System.Collections.Generic;
using UnityEngine;

public class StartPatrolTrigger : MonoBehaviour
{
    [SerializeField] private List<EnemyFsm> enemiesToTrigger = new List<EnemyFsm>();
    
    private void OnTriggerEnter(Collider other)
    {
        print("player entered Trigger");
        foreach (EnemyFsm enemy in enemiesToTrigger)
        {
            enemy.SetPatrolState();
        }
    }
}
