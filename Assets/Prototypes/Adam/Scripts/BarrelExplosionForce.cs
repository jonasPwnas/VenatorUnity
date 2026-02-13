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
        if (other.GetComponent<Rigidbody>() != null && !other.CompareTag("Immovable"))
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
            print(other.gameObject.name);

            IDamageableElementalSource target = other.GetComponent<IDamageableElementalSource>();

            switch (target.GetSourceElement())
            {
                case ElementTypes.ElementType.Fire:
                    target.TakeDamage(damageToDeal * 2, damageType, false);
                    break;
                case ElementTypes.ElementType.Air:
                    target.TakeDamage(damageToDeal / 2, damageType, false);
                    break;
                case ElementTypes.ElementType.Water:
                    target.TakeDamage(damageToDeal * 4, damageType, false);
                    break;
                case ElementTypes.ElementType.NoElement:
                    target.TakeDamage(damageToDeal * 2, damageType, false);
                    break;
            }
        }
    }
}