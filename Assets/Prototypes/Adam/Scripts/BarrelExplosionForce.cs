using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class BarrelExplosionForce : MonoBehaviour
{
    [Header("Explosion Position")] [SerializeField]
    private Transform explosionOrigin;

    [Header("Explosion Settings")] [SerializeField]
    private float explosionForce;
    [SerializeField] private float radius;
    [SerializeField] private float minUpwardsForce;
    [SerializeField] private float maxUpwardsForce;
    
    [Header("Damage settings")]
    [SerializeField] private float damageToDeal = 26f;
    [SerializeField] private ElementTypes.ElementType damageType = ElementTypes.ElementType.Fire;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            Rigidbody rigid = other.GetComponent<Rigidbody>();

            if (rigid.isKinematic)
            {
                rigid.isKinematic = false;
            }

            rigid.AddExplosionForce(explosionForce, explosionOrigin.position, radius,
                Random.Range(minUpwardsForce, maxUpwardsForce), ForceMode.Impulse);

            if(other.gameObject.GetComponent<DestroyAfterExplosion>() != null)
            {
                other.GetComponent<DestroyAfterExplosion>().StartDespawning();
            }
        }

        if (other.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.GetComponent<IDamageableElementalSource>();
            target.TakeDamage(damageToDeal, damageType);
        }
    }
}