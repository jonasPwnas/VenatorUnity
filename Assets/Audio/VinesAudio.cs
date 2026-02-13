using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class VinesAudio : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference FireLoop;
    //Privates
    private EventInstance burningLoop;
    
    private bool m_isBurning = false;

    private void Awake()
    {
        burningLoop = RuntimeManager.CreateInstance(FireLoop);
        RuntimeManager.AttachInstanceToGameObject(burningLoop, gameObject);
    }

    public void StartBurning()
    {
        if (m_isBurning)
            return;
        
        burningLoop.start();
        m_isBurning = true;
    }

    public void StopBurning()
    {
        burningLoop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
