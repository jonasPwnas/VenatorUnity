using UnityEngine;

public class ParentElementSource : MonoBehaviour, IDamageableElementalSource
{

    [SerializeField] private ElementTypes.ElementType elementType = ElementTypes.ElementType.Fire;
    [SerializeField] private int elementAmountToGive = 3;
    [SerializeField] private float elementChargeTime = 2.5f;

    public float ChargeElementTime()
    {
        return elementChargeTime;
    }

    public int EquippedElementAmount()
    {
        return 0;
    }

    public int ElementAmountToGive()
    {
        return elementAmountToGive;
    }

    public ElementTypes.ElementType GetSourceElement()
    {
        return elementType;
    }
    
    public void RemoveElement(ElementTypes.ElementType elementType, int amount)
    {
        //no
    }
    
    public void TakeDamage(float damageAmount, ElementTypes.ElementType elementType)
    {
        //no
    }

    public void Heal(float healAmount, ElementTypes.ElementType elementType)
    {
        //no
    }

    public IDamageableElementalSource.ElementInteractType InteractType()
    {
        return IDamageableElementalSource.ElementInteractType.Source;
    }

    public void AddElement(ElementTypes.ElementType elementType, int amount)
    {
        //no
    }
}
