using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VinesFireSpread : MonoBehaviour
{
    [SerializeField] private float waitToSpreadTime;

    private List<IDamageableElementalSource> fireSpreadObjects;

    private ElementTypes.ElementType elementType = ElementTypes.ElementType.Fire;

    private void Start()
    {
        fireSpreadObjects = new List<IDamageableElementalSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        print("wa WANT spread to: ");
        
        if (other.gameObject.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.gameObject.GetComponent<IDamageableElementalSource>();

            fireSpreadObjects.Add(target);

            StartCoroutine(WaitToSpreadFire());
        }
    }

    private IEnumerator WaitToSpreadFire()
    {
        print("Waiting to spread to: " + fireSpreadObjects.Count);
        yield return new WaitForSeconds(waitToSpreadTime);

        foreach(var target in fireSpreadObjects)
        {
            print("dealt fire-spread damage to " + target);
            target.TakeDamage(10f, elementType);
        }
    }
}
