using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    private GameObject onBoardSpawnPoint;
    private GameObject realSpawnPoint;
    private bool m_postOnboardingStart = false;
    private bool m_shortcutUnlocked = false;
    
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
        
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");
    }

    public void StartPostOnboarding(bool startPostOnboarding)
    {
        //Debug.LogError("WS: Start post onboarding: " + startPostOnboarding);
        m_postOnboardingStart = startPostOnboarding;
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
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");
        
        if(m_postOnboardingStart)
        {
            //Debug.LogError("WS: returned REAL spawn point");
            return realSpawnPoint.transform;
        }
        //Debug.LogError("WS: returned ONBOARDING spawn point");
        return onBoardSpawnPoint.transform;
    }
}