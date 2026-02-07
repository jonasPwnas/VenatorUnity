using System;
using UnityEngine;

public class Test123 : MonoBehaviour
{
    [SerializeField] private float damageToDeal = 10f;
    [SerializeField] private ElementTypes.ElementType damageType = ElementTypes.ElementType.Fire;

    
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.GetComponent<IDamageableElementalSource>();
            target.TakeDamage(damageToDeal, damageType);
            print("damaged:  " + other.name);
        }
    }
}
