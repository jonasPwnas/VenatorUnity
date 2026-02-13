using System;
using System.Collections;
using Player;
using UnityEngine;

public class FogWallTeleport : PlayerObjectInteraction
{
    [SerializeField] private Transform bossTeleportPosition;
    [SerializeField] private WorldAmbAudioPlayer m_ambAudioPlayer;
    
    private bool m_finishedInteract = false;
    private SceneLoader m_sceneLoader;
    private PlayerController m_playerController;
    private WorldSettings m_worldSettings;
    

    public override void Awake()
    {
        base.Awake();
        m_sceneLoader = FindAnyObjectByType<SceneLoader>();
        m_playerController = FindAnyObjectByType<PlayerController>();
    }

    public override void Start()
    {
        base.Start();
        bossTeleportPosition = GameObject.Find("BossLocTeleport").transform;
        m_worldSettings = FindAnyObjectByType<WorldSettings>();
    }

    public override void OnTriggerEnter(Collider other) //tell UI to show pickup prompt here
    {
        print("pick: player enter");
        InvokeTheEvent(true);
    }

    public override void OnTriggerExit(Collider other) //tell UI to show pickup prompt here
    { 
        print("pick: player exit");
        InvokeTheEvent(false);
    }

    public override void DoInteract()
    {
        if (m_finishedInteract)
            return;
        m_ambAudioPlayer.StopWorldAmbiance();
        m_finishedInteract = true;
        m_worldSettings.StartPostOnboarding(true);
        StartCoroutine(GoToBoss());

    }

    IEnumerator GoToBoss()
    {
        m_sceneLoader.FadeCamera(true);
        m_playerController.EnableCharacter(false);
        yield return new WaitForSeconds(1.1f);
        
        m_playerController.transform.position = bossTeleportPosition.position;
        m_sceneLoader.FadeCamera(false);
        yield return new WaitForSeconds(0.7f);
        
        m_playerController.EnableCharacter(true);
    }
}
