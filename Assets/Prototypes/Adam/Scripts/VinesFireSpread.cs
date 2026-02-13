using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VinesFireSpread : MonoBehaviour
{
    private ElementTypes.ElementType elementType = ElementTypes.ElementType.Fire;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.gameObject.GetComponent<IDamageableElementalSource>();

            if (!other.gameObject.CompareTag("Player"))
                target.TakeDamage(10f, elementType, false);
        }
    }
}
