using UnityEngine;

public class BarrelExplodeOnImpact : MonoBehaviour
{
    [SerializeField] private Barrel barrel;

    private bool hasCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasCollided)
        {
            print(collision.gameObject.name);

            hasCollided = true;

            if (collision.gameObject != null)
            {
                barrel.BarrelImpact();
            }
        }        
    }
}
