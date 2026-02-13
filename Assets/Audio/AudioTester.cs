using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference MenuMusic;
    [SerializeField] private EventReference burningSfx;

    
    [SerializeField] private bool playTwoDeeLoop = false;
    
    //Privates
    private EventInstance musicLoopInstance;
    private EventInstance burningLoop;

    private void Awake()
    {
        musicLoopInstance = RuntimeManager.CreateInstance(MenuMusic);
        
        burningLoop = RuntimeManager.CreateInstance(burningSfx);
        RuntimeManager.AttachInstanceToGameObject(burningLoop, gameObject);
    }
    
    private void Start()
    {
        if (playTwoDeeLoop)
            musicLoopInstance.start();
        else
            burningLoop.start();
    }
}
