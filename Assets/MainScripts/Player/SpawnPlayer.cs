using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayer : MonoBehaviour
{
    public delegate void PlayerRevive();
    public static event PlayerRevive OnPlayerRevive;
    [SerializeField] private GameObject actualDeathMenu;
    
    private PlayerStatus playerStatus;
    private GameObject onBoardSpawnPoint;
    private GameObject realSpawnPoint;
    private PlayerController playerController;
    private ThirdPersonCamera thirdPersonCamera;
    private Vector3 respawnLocation;
    private DeathMenu deathMenu;
    PauseManager pauseManager;
    Animator playerAnimator;

    private void Awake()
    {
        playerController = GameObject.FindAnyObjectByType<PlayerController>();
        thirdPersonCamera = GameObject.FindAnyObjectByType<ThirdPersonCamera>();
        pauseManager = FindAnyObjectByType<PauseManager>();
        deathMenu = FindAnyObjectByType<DeathMenu>();
        playerAnimator = playerController.GetComponent<Animator>();
        playerStatus = FindAnyObjectByType<PlayerStatus>();
        onBoardSpawnPoint = GameObject.Find("OnBoardSpawnPoint");
        realSpawnPoint = GameObject.Find("FirstSpawnPoint");

        respawnLocation = playerController.transform.position;

        PlaceCharacter();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        //PlayerStatus.OnHealed += RespawnCharacter;
    }

    private void OnDisable()
    {
        //PlayerStatus.OnHealed -= RespawnCharacter;
    }

    public void Respawn()
    {
        playerController.transform.position = realSpawnPoint.transform.position;
        thirdPersonCamera.transform.position = realSpawnPoint.transform.position;
        OnPlayerRevive?.Invoke();
        deathMenu.gameObject.SetActive(false);
        StartCoroutine(UglyRespawnForTest(2f));
    }

    private void PlaceCharacter()
    {
        playerController.transform.position = onBoardSpawnPoint.transform.position;
        thirdPersonCamera.transform.position = onBoardSpawnPoint.transform.position;
    }

    public void RespawnCharacter(float amount, ElementTypes.ElementType elementType)
    {

    }

    IEnumerator UglyRespawnForTest(float dumbWait)
    {
        yield return new WaitForSeconds(dumbWait);
        playerController.RevivePlayer();
        actualDeathMenu.SetActive(false);
        deathMenu.gameObject.SetActive(true);
        playerStatus.SetNotDead();
        StartCoroutine(ExtraUglyRespawnForTest(2f));
    }

    IEnumerator ExtraUglyRespawnForTest(float dumbWait)
    {
        playerController.TriggerHeal();
        yield return new WaitForSeconds(dumbWait);
        playerController.TriggerHeal();
        playerStatus.Heal(100f, ElementTypes.ElementType.Water);
    }
    
}
