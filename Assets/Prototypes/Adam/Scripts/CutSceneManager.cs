using System.Collections;
using Unity.Cinemachine;
using Unity.Hierarchy;
using UnityEngine;

public class CutSceneManager : MonoBehaviour
{
    //Delegates
    public delegate void StartCutscene();
    public static event StartCutscene OnStartCutscene;

    public delegate void EndCutscene();
    public static event EndCutscene OnEndCutscene;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private GameObject cutsceneGameObject;
    [SerializeField] private CinemachineSplineDolly cameraTrackLogic;

    [Header("Cut Scene Settings")]
    [SerializeField] private float cutsceneDowntime;
    
    private bool cutsceneIsDone = false;

    private void Start()
    {
        cameraTrackLogic.AutomaticDolly.Enabled = false;
        cutsceneCamera.enabled = false;
    }

    private void Update()
    {
        CutsceneDone();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && other != null)
        {
            Debug.Log("Triggered");

            OnStartCutscene?.Invoke();

            cutsceneCamera.enabled = true;
            playerCamera.enabled = false;

            StartCoroutine(MoveCamera(cutsceneDowntime));
        }
    }

    private IEnumerator MoveCamera(float cutsceneDowntime)
    {
        cameraTrackLogic.AutomaticDolly.Enabled = true;

        yield return new WaitForSeconds(cutsceneDowntime);

        cutsceneCamera.enabled = false;
        cutsceneIsDone = true;

        OnEndCutscene?.Invoke();
    }

    private void CutsceneDone()
    {
        if (cameraTrackLogic.CameraPosition >= 1 && cutsceneIsDone)
        {
            cutsceneIsDone = false;
            playerCamera.enabled = true;
            Destroy(cutsceneGameObject);
        }
    }
}
