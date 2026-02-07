using UnityEngine;

public class FireBallSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject fireBallProjectile;
    [SerializeField] Transform spawnPoint;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed;

    [Header("Debuggin")]
    [SerializeField] private bool oneProjectile;

    // Remove Update Function for the Keybind Callout
    private void Update()
    {
        if (oneProjectile)
            SpawnFireBall();
    }

    public void SpawnFireBall()
    {
        var airProjectileInstance = Instantiate(fireBallProjectile, spawnPoint.position, spawnPoint.rotation);

        airProjectileInstance.GetComponent<Rigidbody>().AddForce(airProjectileInstance.transform.forward * projectileSpeed);

        // Debugging
        oneProjectile = false;
    }
}
