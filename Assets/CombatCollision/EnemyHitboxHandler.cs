using System;
using UnityEngine;

public class EnemyHitboxHandler : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private float damageToDeal = 25f; //get this from player config instead?
    [SerializeField] private EnemyStatus enemyStatus; //get this from player config instead?

    //privates
    private bool m_hitboxEnabled = false;
    private Collider m_hitbox;


    private void Awake()
    {
        m_hitbox = GetComponent<Collider>();
    }
    
    public void DoToggleHitbox(bool enableBox)
    {
        if (enableBox)
        {
            m_hitboxEnabled = true;
            m_hitbox.enabled = m_hitboxEnabled;
            return;
        }
        if (!enableBox)
        {
            m_hitboxEnabled = false;
            m_hitbox.enabled = m_hitboxEnabled;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageableElementalSource>() != null)
        {
            print("Dealt damage to: " + other.name);
            
            IDamageableElementalSource target = other.GetComponent<IDamageableElementalSource>();
            target.TakeDamage(damageToDeal, enemyStatus.GetSourceElement());
        }
    }
}
