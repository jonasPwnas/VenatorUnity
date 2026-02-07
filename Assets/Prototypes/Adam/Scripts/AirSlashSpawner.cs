using Player;
using UnityEngine;

public class AirSlashSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject airProjectile;
    [SerializeField] Transform spawnPoint;
    [SerializeField] SphereCollider projectileCollider;

    [Header("Air Projectile Settings")]
    [SerializeField] private float projectileSpeed;

    private void OnEnable()
    {
        PlayerController.OnCastSpell += SpawnAirProjectile;
    }

    private void OnDisable()
    {
        PlayerController.OnCastSpell -= SpawnAirProjectile;
    }

    public void SpawnAirProjectile(ElementTypes.ElementType sourceElement)
    {
        if(sourceElement == ElementTypes.ElementType.Air)
        {
            var airProjectileInstance = Instantiate(airProjectile, spawnPoint.position, spawnPoint.rotation);

            airProjectileInstance.GetComponent<Rigidbody>().AddForce(airProjectileInstance.transform.forward * projectileSpeed);
        }
    }
}
