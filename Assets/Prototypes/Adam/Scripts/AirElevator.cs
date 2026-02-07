using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AirElevator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference the Elevator")]
    [SerializeField] private GameObject affectedElevator;
    [Tooltip("Reference the Air Trigger Object")]
    [SerializeField] private GameObject airTriggerObject;

    [Header("Variables")]
    [Tooltip("Elevators time to move to the designated position.")]
    [SerializeField] private float elevatorDuration = 2f;
    [Tooltip("How high/low the elevator will go.")]
    [SerializeField] private float verticalStopPosition;
    [Tooltip("True = Move Up and False = Move Down")]
    [SerializeField] private bool movingUp;
    [Tooltip("Seconds it will take from triggering to activating the elevator")]
    [SerializeField] private float waitForElevator;
    [Tooltip("End location of elevator")]
    [SerializeField] private Transform endObject;


    private Vector3 startingLocation;
    private Vector3 endLocation;

    private bool m_isAtEnd = false;
    private bool elevatorActive = false;

    private void Start()
    {
        startingLocation = affectedElevator.transform.position;
        endLocation = endObject.position;
    }


    public void ActivateElevator()
    {
        ElevatorMovement();
    }

    private void Update()
    {
        if (elevatorActive)
        {
            AirTriggerSpinning();
        }
    }

    private void ElevatorMovement()
    {
        if (elevatorActive)
            return;
        elevatorActive = true;
        StartCoroutine(ElevatorTiming());
    }

    private void AirTriggerSpinning()
    {
        if(!m_isAtEnd)
            airTriggerObject.transform.Rotate(0, elevatorDuration * 10 * Time.deltaTime, 0, Space.World);
        else
            airTriggerObject.transform.Rotate(0, elevatorDuration * -10 * Time.deltaTime, 0, Space.World);
    }

    IEnumerator ElevatorTiming()
    {
        if (!m_isAtEnd)
        {
            affectedElevator.transform.DOMove(endLocation, elevatorDuration);
            m_isAtEnd = true;
        }
        else
        {
            affectedElevator.transform.DOMove(startingLocation, elevatorDuration);
            m_isAtEnd = false;
        }
        yield return new WaitForSeconds(elevatorDuration);
        elevatorActive = false;
    }

}