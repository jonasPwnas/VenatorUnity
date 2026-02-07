using System;
using UnityEngine;

public class PlayerHitboxHandler : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private float damageToDeal = 25f; //get this from player config instead?
    [SerializeField] private PlayerStatus elementalSource; //get this from player config instead?

    //privates
    private bool m_hitboxEnabled = false;
    private Collider m_hitbox;
    private PlayerStatus m_playerStatus;


    private void Awake()
    {
        m_hitbox = GetComponent<Collider>();
        m_playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    private void OnEnable()
    {
        PlayerAttackEventReciever.OnToggleHitbox += DoToggleHitbox;
    }

    private void OnDisable()
    {
        PlayerAttackEventReciever.OnToggleHitbox -= DoToggleHitbox;
    }

    private void DoToggleHitbox()
    {
        if (!m_hitboxEnabled)
        {
            //print("ENABLED MURDER LÅDDA");
            m_hitboxEnabled = true;
            m_hitbox.enabled = m_hitboxEnabled;
            return;
        }
        if (m_hitboxEnabled)
        {
            //print("DISABLED MURDER LÅDDAnnnnnnnn");
            m_hitboxEnabled = false;
            m_hitbox.enabled = m_hitboxEnabled;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.GetComponent<IDamageableElementalSource>();
            
            switch (target.InteractType())
            {
                case IDamageableElementalSource.ElementInteractType.Enemy:
                    if(m_playerStatus.EquippedElementAmount() == 0)
                    {
                        switch (m_playerStatus.GetSourceElement())
                        {
                            case ElementTypes.ElementType.Fire:
                                target.TakeDamage(damageToDeal, ElementTypes.ElementType.Fire);
                                break;
                            case ElementTypes.ElementType.Water:
                                target.TakeDamage(damageToDeal, ElementTypes.ElementType.Water);
                                break;
                            case ElementTypes.ElementType.Air:
                                target.TakeDamage(damageToDeal, ElementTypes.ElementType.Air);
                                break;
                            case ElementTypes.ElementType.NoElement:
                                target.TakeDamage(damageToDeal, ElementTypes.ElementType.NoElement);
                                break;
                        }
                        break;
                    }
                    target.TakeDamage(damageToDeal, elementalSource.GetSourceElement());
                    break;
                case IDamageableElementalSource.ElementInteractType.User:
                    if(m_playerStatus.EquippedElementAmount() == 0)
                    {
                        target.TakeDamage(damageToDeal, ElementTypes.ElementType.NoElement);
                        break;
                    }
                    target.TakeDamage(damageToDeal, elementalSource.GetSourceElement());
                    break;
                case IDamageableElementalSource.ElementInteractType.Source: //Change this to be a generalized "charging" of elements, not attacks, seperate button/logic
                    break;
            }
        }
    }
}
