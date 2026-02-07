using UnityEngine;

public class ElementTriggerEvent : ParentElementUser
{
    [Header("Variables")]
    [SerializeField] AirElevator airElevator;
    [SerializeField] Vines vines;

    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        switch (elementType)
        {
            case ElementTypes.ElementType.Air:
                airElevator.ActivateElevator();
                break;
            case ElementTypes.ElementType.Fire:
                vines.fireActive = true;
                break;
        }

    }
}
