using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MenuAudio : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference MenuMusic;
    [SerializeField] private EventReference MenuNav;
    [SerializeField] private EventReference MenuAccept;

    //Privates
    private EventInstance musicLoopInstance;

    private void Awake()
    {
        musicLoopInstance = RuntimeManager.CreateInstance(MenuMusic);
    }

    private void Start()
    {
        musicLoopInstance.start();
    }

    private void OnDestroy()
    {
        musicLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PlayNavSound()
    {
        RuntimeManager.PlayOneShot(MenuNav);
    }

    public void PlayAcceptSound()
    {
        RuntimeManager.PlayOneShot(MenuAccept);
    }
}
