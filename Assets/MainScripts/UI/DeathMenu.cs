using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public GameObject DeathMenuPanel;
    SceneLoader sceneLoader;
    PauseManager pauseManager;
    bool hasDied;
    SpawnPlayer m_spawnPlayer;

    private void Awake()
    {
        sceneLoader = FindAnyObjectByType<SceneLoader>();
        pauseManager = FindAnyObjectByType<PauseManager>();
        m_spawnPlayer = FindAnyObjectByType<SpawnPlayer>();
    }

    private void Start()
    {
        DeathMenuPanel.SetActive(false);
    }
    private void OnEnable()
    {
        PlayerStatus.OnPlayerDied += OpenDeathMenu;
    }

    private void OnDisable()
    {
        PlayerStatus.OnPlayerDied -= OpenDeathMenu;
    }

    private void OpenDeathMenu()
    {
        hasDied = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DeathMenuPanel.SetActive(true);
    }

    public void Restart()
    {
        Cursor.visible = false; 
        Cursor.lockState -= CursorLockMode.Locked;
        m_spawnPlayer.Respawn();
    }

    public void Quit()
    {
        pauseManager.UnPause();
        sceneLoader.LoadPreviousScene();
    }
}
