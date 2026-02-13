using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ElevatorAudio : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference MovementLoop;
    [SerializeField] private EventReference StartElevator;
    [SerializeField] private EventReference StopElevator;
    [SerializeField] private EventReference GatesOpenClose;

    //Privates
    private EventInstance m_movementLoop;

    private void Awake()
    {
        m_movementLoop = RuntimeManager.CreateInstance(MovementLoop);
        RuntimeManager.AttachInstanceToGameObject(m_movementLoop, gameObject);
    }

    public void PushedPressurePlate(Vector3 pressurePlatePosition)
    {
        RuntimeManager.PlayOneShot(StartElevator, pressurePlatePosition);
    }
    
    public void ResetPressurePlate(Vector3 pressurePlatePosition)
    {
        RuntimeManager.PlayOneShot(StopElevator, pressurePlatePosition);
    }
    
    public void StartMoving()
    {
        m_movementLoop.start();
    }

    public void StopMoving()
    {
        m_movementLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
