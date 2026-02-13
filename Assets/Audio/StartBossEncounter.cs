using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class StartBossEncounter : MonoBehaviour
{
    //Editor exposed
    [SerializeField] private EventReference BossMusic;
    private Collider m_trigger;
    
    //Privates
    private EventInstance musicLoopInstance;
    private EnemyHealthBarManager m_barManager;
    private WorldSettings m_worldSettings;
    
    private void Awake()
    {
        musicLoopInstance = RuntimeManager.CreateInstance(BossMusic);
        m_trigger = GetComponent<Collider>();
        m_barManager = FindAnyObjectByType<EnemyHealthBarManager>();
    }

    private void Start()
    {
        m_worldSettings = FindAnyObjectByType<WorldSettings>();
    }

    private void OnTriggerEnter(Collider other)
    {
        musicLoopInstance.start();
        m_barManager.StartBossHealthBar();
        m_trigger.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        musicLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
