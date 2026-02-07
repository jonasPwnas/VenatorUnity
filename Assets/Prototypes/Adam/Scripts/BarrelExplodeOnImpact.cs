using UnityEngine;

public class BarrelExplodeOnImpact : MonoBehaviour
{
    public bool barrelImpact;

    private void OnCollisionEnter(Collision collision)
    {
        print("Barrel Impact");

        if (collision.gameObject != null)
        {
            
            barrelImpact = true;
        }
    }
}
