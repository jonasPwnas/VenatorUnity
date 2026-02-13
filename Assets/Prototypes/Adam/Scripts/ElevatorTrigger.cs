using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class ElevatorTrigger : MonoBehaviour
{
    [SerializeField] private Elevator elevator;
    [SerializeField] private ElevatorAudio elevatorAudio;

    [Range(0, 10)]
    [SerializeField] private float waitBeforeMoving;

    [SerializeField] private Collider pressurePlateTrigger;
    [SerializeField] private GameObject pressurePlate;
    [SerializeField] private Transform pressurePlateDown;

    [SerializeField] private GameObject gateOne;
    [SerializeField] private GameObject gateTwo;
    [SerializeField] private GameObject gateThree;
    [SerializeField] private GameObject gateFour;

    private Vector3 endPosition;
    private Vector3 startPosition;

    private Vector3 gateOneStartPosition = new Vector3(0, -280, 0);
    private Vector3 gateTwoStartPosition = new Vector3(0, -260, 0);
    private Vector3 gateThreeStartPosition = new Vector3(0,250, 0);
    private Vector3 gateFourStartPosition = new Vector3(0, 290, 0);

    private Vector3 gateOneEndPosition = new Vector3(0, -180, 0);
    private Vector3 gateTwoEndPosition = new Vector3(0, 0, 0);
    private Vector3 gateThreeEndPosition = new Vector3(0, 0, 0);
    private Vector3 gateFourEndPosition = new Vector3(0, -180, 0);

    private void Start()
    {
        startPosition = pressurePlate.transform.localPosition;
        endPosition = pressurePlateDown.localPosition;
        StartCoroutine(OpenCorrectGates());
    }

    private void OnTriggerEnter(Collider other)
    {
        print("PLayer entered");

        if (!elevator.elevatorIsActive)
            StartCoroutine(WaitBeforeStarting());

        elevatorAudio.PushedPressurePlate(endPosition);

        pressurePlate.transform.DOLocalMove(endPosition, waitBeforeMoving);

        gateOne.transform.DOLocalRotate(gateOneEndPosition, waitBeforeMoving);
        gateTwo.transform.DOLocalRotate(gateTwoEndPosition, waitBeforeMoving);

        gateThree.transform.DOLocalRotate(gateThreeEndPosition, waitBeforeMoving);
        gateFour.transform.DOLocalRotate(gateFourEndPosition, waitBeforeMoving);

        pressurePlateTrigger.enabled = false;
    }

    private IEnumerator WaitBeforeStarting()
    {
        yield return new WaitForSeconds(waitBeforeMoving);

        StartElevator();
    }

    private IEnumerator PressurePlateReset()
    {
        yield return new WaitForSeconds(elevator.elevatorDurtaion);

        elevatorAudio.ResetPressurePlate(startPosition);

        pressurePlate.transform.DOLocalMove(startPosition, waitBeforeMoving);

        if (elevator.movingToEnd)
        {
            gateOne.transform.DOLocalRotate(gateOneStartPosition, waitBeforeMoving);
            gateTwo.transform.DOLocalRotate(gateTwoStartPosition, waitBeforeMoving);
        }
        else if (!elevator.movingToEnd)
        {
            gateThree.transform.DOLocalRotate(gateThreeStartPosition, waitBeforeMoving);
            gateFour.transform.DOLocalRotate(gateFourStartPosition, waitBeforeMoving);
        }

        yield return new WaitForSeconds(waitBeforeMoving);

        yield return new WaitForSeconds(2f);

        pressurePlateTrigger.enabled = true;
    }

    private void StartElevator()
    {
        if (elevator.movingToEnd)
        {
            elevator.ElevatorMoveToEnd();
        }
        else if (!elevator.movingToEnd)
        {
            elevator.ElevatorMoveToStart();
        }

        StartCoroutine(PressurePlateReset());
    }

    IEnumerator OpenCorrectGates()
    {
        yield return new WaitForSeconds(1f);
        
        if (elevator.movingToEnd)
        {
            gateOne.transform.DOLocalRotate(gateOneStartPosition, waitBeforeMoving);
            gateTwo.transform.DOLocalRotate(gateTwoStartPosition, waitBeforeMoving);
            gateThree.transform.DOLocalRotate(gateThreeEndPosition, waitBeforeMoving);
            gateFour.transform.DOLocalRotate(gateFourEndPosition, waitBeforeMoving);
            
        }
        else if (!elevator.movingToEnd)
        {
            gateOne.transform.DOLocalRotate(gateOneEndPosition, waitBeforeMoving);
            gateTwo.transform.DOLocalRotate(gateTwoEndPosition, waitBeforeMoving);
            gateThree.transform.DOLocalRotate(gateThreeStartPosition, waitBeforeMoving);
            gateFour.transform.DOLocalRotate(gateFourStartPosition, waitBeforeMoving);
        }
    }
}
