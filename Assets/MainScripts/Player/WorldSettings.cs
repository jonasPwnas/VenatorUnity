using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    private GameObject bossLocation;
    private GameObject onBoardSpawnPoint;
    private GameObject realSpawnPoint;
    private bool m_postOnboardingStart = false;
    private bool m_shortcutUnlocked = false;
    private bool m_startAtBoss = false;

    [HideInInspector] public bool bossUseHalfHp = false;
    
    public static WorldSettings Instance;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = this; 
        } 
        else 
        { 
            Instance = this; 
        } 
        
        DontDestroyOnLoad(gameObject);
        bossLocation = GameObject.Find("BossLocTeleport");
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");
    }

    public void StartAtBoss(bool startAtBoss)
    {
        m_startAtBoss = startAtBoss;
        bossUseHalfHp = true;
    }
    
    public void StartPostOnboarding(bool startPostOnboarding)
    {
        //Debug.LogError("WS: Start post onboarding: " + startPostOnboarding);
        m_postOnboardingStart = startPostOnboarding;
        bossUseHalfHp = true;
    }
    
    public void SetShortcutUnlocked(bool shortcutUnlocked)
    {
        //Debug.LogError("WS: Shortcut Unlocked " +  shortcutUnlocked);
        m_shortcutUnlocked = shortcutUnlocked;
    }
    
    public bool GetIsShortcutUnlocked()
    {
        return m_shortcutUnlocked;
    }
    
    public Transform GetSpawnTransform()
    {
        bossLocation = GameObject.Find("BossLocTeleport");
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");

        if (m_startAtBoss)
        {
            return bossLocation.transform;
        }
        
        if(m_postOnboardingStart)
        {
            return realSpawnPoint.transform;
        }
        
        return onBoardSpawnPoint.transform;
    }
}