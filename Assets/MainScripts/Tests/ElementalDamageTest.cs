using UnityEngine;

public class ElementalDamageTest : ParentElementUser
{
    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        switch (elementType)
        {
            case ElementTypes.ElementType.Air:
                print("AIR damage YAY!!");
                break;
            case ElementTypes.ElementType.Fire:
                print("FIRE damage YAY!!");
                break;
            case ElementTypes.ElementType.Water:
                print("WATER damage YAY!!");
                break;
            case ElementTypes.ElementType.NoElement:
                print("PHYSICAL damage YAY!!");
                break;
        }
    }
}
