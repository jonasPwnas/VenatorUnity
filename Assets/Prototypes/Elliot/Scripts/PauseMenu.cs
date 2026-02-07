using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]GameObject pauseMenuUI;
    [SerializeField] GameObject uiInput;
    PauseManager pauseManager; 
    SceneLoader sceneLoader;

    private void Start()
    {
        pauseManager = FindAnyObjectByType<PauseManager>();
        sceneLoader = FindAnyObjectByType<SceneLoader>();
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuUI.SetActive(false);

    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (pauseManager.isPaused)
        {
            pauseManager.UnPause();
            pauseMenuUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            pauseManager.Pause();
            pauseMenuUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        pauseManager.UnPause();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("Resuming Game");
    }

    public void QuitGame()
    {
        pauseManager.UnPause();
        sceneLoader.LoadPreviousScene();
    }


}
