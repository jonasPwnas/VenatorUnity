using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Elevator : MonoBehaviour
{
    [SerializeField] private ElevatorAudio elevatorAudio;

    [Header("References")]
    [SerializeField] private GameObject affectedElevator;
    [SerializeField] private GameObject spinningMillOne;
    [SerializeField] private GameObject spinningMillTwo;
    [SerializeField] public Transform endPosition;
    [SerializeField] public float elevatorDurtaion;

    private Vector3 m_startingLocation;
    private Vector3 m_endLocation;
    
    public bool movingToEnd;
    public bool elevatorIsActive;

    private void Awake()
    {
        m_startingLocation = affectedElevator.transform.position;
        m_endLocation = endPosition.position;

        movingToEnd = true;
    }

    private void Update()
    {
        if (elevatorIsActive)
        {
            MillSpinning();
        }
    }

    public void ElevatorMoveToEnd()
    {
        affectedElevator.transform.DOMove(m_endLocation, elevatorDurtaion);
        
        StartCoroutine(ElevatorActive());

        movingToEnd = false;
    }

    public void ElevatorMoveToStart()
    {
        affectedElevator.transform.DOMove(m_startingLocation, elevatorDurtaion);

        StartCoroutine(ElevatorActive());

        movingToEnd = true;
    }

    private void MillSpinning()
    {
        if (!movingToEnd)
        {
            spinningMillOne.transform.Rotate(0, elevatorDurtaion * 10 * Time.deltaTime, 0, Space.Self);
            spinningMillTwo.transform.Rotate(0, elevatorDurtaion * -10 * Time.deltaTime, 0, Space.Self);
        }
        else
        {
            spinningMillOne.transform.Rotate(0, elevatorDurtaion * -10 * Time.deltaTime, 0, Space.Self);
            spinningMillTwo.transform.Rotate(0, elevatorDurtaion * 10 * Time.deltaTime, 0, Space.Self);
        }
    }

    private IEnumerator ElevatorActive()
    {
        elevatorAudio.StartMoving();

        elevatorIsActive = true;

        yield return new WaitForSeconds(elevatorDurtaion);

        elevatorAudio.StopMoving();

        elevatorIsActive = false;
    }
}
