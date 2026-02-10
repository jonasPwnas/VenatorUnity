using System;
using UnityEngine;

public class SetShortcutElevatorPosition : MonoBehaviour
{
    [SerializeField] ElevatorTrigger elevatorTrigger;
    [SerializeField] Elevator elevatorLogicObj;
    [SerializeField] GameObject elevatorComponent;


    private void Start()
    {
        if (WorldSettings.Instance.GetIsShortcutUnlocked())
            return;
        
        elevatorComponent.transform.position = elevatorLogicObj.endPosition.position;
        elevatorLogicObj.movingToEnd = false;
    }
}
