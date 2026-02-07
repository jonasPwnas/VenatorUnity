using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [Header("Screens")]
    public CanvasGroup startScreen;
    public CanvasGroup mainMenu;
    public CanvasGroup optionsModal;
    public CanvasGroup gameView;

    [SerializeField] private InputAction AnyInput;
    SceneLoader sceneLoader;
    private bool inMenu = false;

    private void Awake()
    {
        AnyInput.Enable();
        sceneLoader = FindAnyObjectByType<SceneLoader>();
    }

    void Start()
    {
        // Initial State: Show Start Screen, Hide others
        SetAlpha(startScreen, 1);
        SetAlpha(mainMenu, 0);
        SetAlpha(optionsModal, 0);
        SetAlpha(gameView, 0);
    }

    void Update()
    {
        // Press Any Key logic
        if (!inMenu && AnyInput.triggered)
        {
            StartCoroutine(TransitionToMenu());
        }
    }

    public void OnStartGame()
    {
        StopAllCoroutines();
        //StartCoroutine(FadeSequence(mainMenu, 0));
        //StartCoroutine(FadeSequence(gameView, 1));
        sceneLoader.LoadNextScene();
        Debug.Log("Game Started!");
    }

    public void OnOptions()
    {
        StartCoroutine(FadeSequence(optionsModal, 1));
    }

    public void OnCloseOptions()
    {
        StartCoroutine(FadeSequence(optionsModal, 0));
    }

    public void OnExit()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }

    private IEnumerator TransitionToMenu()
    {
        inMenu = true;
        yield return StartCoroutine(FadeSequence(startScreen, 0));
        yield return StartCoroutine(FadeSequence(mainMenu, 1));
    }

    private IEnumerator FadeSequence(CanvasGroup cg, float targetAlpha)
    {
        if (cg == null) yield break;

        float start = cg.alpha;
        float time = 0;
        float duration = 0.5f;

        cg.blocksRaycasts = (targetAlpha > 0.5f); // Enable interaction only if visible

        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, targetAlpha, time / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    private void SetAlpha(CanvasGroup cg, float alpha)
    {
        if (cg) 
        {
            cg.alpha = alpha;
            cg.blocksRaycasts = (alpha > 0.5f);
        }
    }

    private void OnEnable()
    {
        AnyInput.Enable();
    }

    private void OnDisable()
    {
        AnyInput.Disable();
    }
}
