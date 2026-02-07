using UnityEngine;

public class ParentElementUser : MonoBehaviour, IDamageableElementalSource
{
    [Header("What element needed to activate")]
    [SerializeField] public ElementTypes.ElementType activationElement = ElementTypes.ElementType.Fire;
    [Header("How much damage to activate?")]
    [SerializeField] private float damageActivationThreshold = 10f;
    
    public virtual void TakeDamage(float damageAmount, ElementTypes.ElementType elementType)
    { 
        OnActivated(damageAmount, elementType);
    }

    public virtual void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        
    }
    
    public IDamageableElementalSource.ElementInteractType InteractType()
    {
        return IDamageableElementalSource.ElementInteractType.User;
    }

    public int EquippedElementAmount()
    {
        return 0;
    }


    //beneath is not applicable
    public float ChargeElementTime()
    {
        return 0f;
    }
    public void Heal(float healAmount, ElementTypes.ElementType elementType)
    {
        //no
    }
    public int ElementAmountToGive()
    {
        //no
        return 0;
    }
    public ElementTypes.ElementType GetSourceElement()
    {
        //no
        return ElementTypes.ElementType.NoElement;
    }
    public void AddElement(ElementTypes.ElementType elementType, int amount)
    {
        //no
    }
    public void RemoveElement(ElementTypes.ElementType elementType, int amount)
    {
        //no
    }
}
