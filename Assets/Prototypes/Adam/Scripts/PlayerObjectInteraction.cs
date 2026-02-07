using UnityEngine;

public class PlayerObjectInteraction : MonoBehaviour
{
    public delegate void PickUp(bool canPickup);
    public static event PickUp OnPickUp;

    [Tooltip("The Given Element Type")]
    [SerializeField] private ElementTypes.ElementType m_elementType = ElementTypes.ElementType.NoElement;
    [Tooltip("The Amount of Sources Dropped")]
    [SerializeField] private int elementAmountToGivePlayer = 1;
    [Tooltip("The Source Object")]
    [SerializeField] private GameObject elementDropPrefab;

    [SerializeField] private GameObject glowingSphere;
    [SerializeField] private GameObject dropLogicObject;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject != null)
        {
            OnPickUp?.Invoke(true);

            // Call this on the button input
            DropElementCharges();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject != null)
        {
            OnPickUp?.Invoke(false);
        }
    }

    private void DropElementCharges()
    {
        Destroy(glowingSphere);
        Destroy(dropLogicObject);

        for (int i = 0; i < elementAmountToGivePlayer; i++)
        {
            DropdElementalCharge charge = Instantiate(elementDropPrefab, transform.position, Quaternion.identity).GetComponent<DropdElementalCharge>();
            charge.SetElementType(m_elementType);

        }
    }
}
