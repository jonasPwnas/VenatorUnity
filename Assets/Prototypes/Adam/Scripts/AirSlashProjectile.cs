using System.Collections;
using UnityEngine;

public class AirSlashProjectile : MonoBehaviour
{
    [Header("Air Push Settings")]
    [SerializeField] private float airPushForce;
    [SerializeField] private float airProjectileDamage = 10f;

    [Header("References")]
    [SerializeField] GameObject projectile;
    
    private ElementTypes.ElementType elementType = ElementTypes.ElementType.Air;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<IDamageableElementalSource>() != null)
        {
            IDamageableElementalSource target = other.gameObject.GetComponent<IDamageableElementalSource>();

            target.TakeDamage(airProjectileDamage, elementType);

            if (other.gameObject.GetComponent<Rigidbody>() != null)
            {
                Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();

                if (targetRb.isKinematic)
                {
                    targetRb.isKinematic = false;
                }

                targetRb.AddForce(projectile.transform.forward * airPushForce);
            }
            else if (other.gameObject.GetComponent<CharacterController>() != null)
            {
                StartCoroutine(MoveEnemy(other.gameObject.GetComponent<CharacterController>(), projectile.transform.forward, other.GetComponent<EnemyStatus>().airPushImpactRatio, 0.05f));
            }
        }
        else
        {
            if (other.gameObject.GetComponent<Rigidbody>() != null && !other.CompareTag("Immovable"))
            {
                if (other.gameObject.GetComponent<Rigidbody>().isKinematic)
                {
                    other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                }

                other.gameObject.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * airPushForce);
            }

            Destroy(projectile);
        }
    }

    private IEnumerator MoveEnemy(CharacterController targetController, Vector3 projectileHitDirection, float move, float timeBetweenMove)
    {
        int timesToMove = 5;

        for(int i = 0; i <= timesToMove; i++)
        {
            targetController.Move(projectileHitDirection * move);

            yield return new WaitForSeconds(timeBetweenMove);
        }

        Destroy(projectile);
    }
}


