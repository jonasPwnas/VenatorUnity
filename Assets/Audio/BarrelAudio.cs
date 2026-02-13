using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class BarrelAudio : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference FireLoop;
    [SerializeField] private EventReference Explosion;
    [SerializeField] private EventReference FireAttacked;
    [SerializeField] private EventReference AirAttacked;
    [SerializeField] private EventReference WaterAttacked;

    //Privates
    private EventInstance burningLoop;

    private void Awake()
    {
        burningLoop = RuntimeManager.CreateInstance(FireLoop);
        RuntimeManager.AttachInstanceToGameObject(burningLoop, gameObject);
    }

    public void Attacked(ElementTypes.ElementType elementType)
    {
        switch (elementType)
        {
            case ElementTypes.ElementType.Fire:
                RuntimeManager.PlayOneShot(FireAttacked, transform.position);
                break;
            case ElementTypes.ElementType.Air:
                RuntimeManager.PlayOneShot(AirAttacked, transform.position);
                break;
            case ElementTypes.ElementType.Water:
                RuntimeManager.PlayOneShot(WaterAttacked, transform.position);
                break;
        }
    }

    public void Explode()
    {
        RuntimeManager.PlayOneShot(Explosion, transform.position);
    }
    
    public void StartBurning()
    {
        burningLoop.start();
    }

    public void StopBurning()
    {
        burningLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDestroy()
    {
        burningLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
