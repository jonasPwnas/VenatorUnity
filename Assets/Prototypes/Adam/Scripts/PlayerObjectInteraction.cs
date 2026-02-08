using UnityEngine;

public class PlayerObjectInteraction : MonoBehaviour
{
    public delegate void PickUp(bool canPickup, PlayerObjectInteraction playerObjectInteraction);
    public static event PickUp OnPickUp;

    [Tooltip("The Given Element Type")]
    [SerializeField] private ElementTypes.ElementType m_elementType = ElementTypes.ElementType.NoElement;
    [Tooltip("The Amount of Sources Dropped")]
    [SerializeField] private int elementAmountToGivePlayer = 1;
    [Tooltip("The Source Object")]
    [SerializeField] private GameObject elementDropPrefab;

    [SerializeField] private GameObject glowingSphere;
    [SerializeField] private GameObject dropLogicObject;
    
    private bool isPickedUp = false;


    private void OnTriggerEnter(Collider other) //tell UI to show pickup prompt here
    {
        print("pick: player enter");
        OnPickUp?.Invoke(true, this);
    }

    private void OnTriggerExit(Collider other) //tell UI to show pickup prompt here
    { 
        print("pick: player exit");
        OnPickUp?.Invoke(false,this);
        
        if(isPickedUp)
            Destroy(dropLogicObject);
    }

    public void PickUpElementCharges()
    {
        if (isPickedUp)
            return;
        
        Destroy(glowingSphere);
        
        for (int i = 0; i < elementAmountToGivePlayer; i++)
        {
            DropdElementalCharge charge = Instantiate(elementDropPrefab, transform.position, Quaternion.identity).GetComponent<DropdElementalCharge>();
            charge.SetElementType(m_elementType);
        }

        isPickedUp = true;
    }
}
