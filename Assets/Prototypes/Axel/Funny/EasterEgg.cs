using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using System.Collections;
using static EnemyStatus;

public class EasterEgg : MonoBehaviour
{
    private HUDController m_hudController;
    private GameObject m_player;
    private VideoPlayer m_videoPlayer;


    private void Start()
    {
        m_hudController = FindAnyObjectByType<HUDController>();
        m_player = FindAnyObjectByType<PlayerController>().gameObject;
        m_videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        EnemyStatus.OnBossIsDeaderThanDisco += WIN;
    }
    private void OnDisable()
    {
        EnemyStatus.OnBossIsDeaderThanDisco -= WIN;
    }


    private void WIN()
    {
        if (m_hudController != null)
        {
            foreach (Transform child in m_hudController.transform)
            {
                child.gameObject.SetActive(false);
            }
            this.transform.parent = m_hudController.transform;
            m_videoPlayer.Play();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_player.GetComponentInParent<PlayerInput>().SwitchCurrentActionMap("UI");
                StartCoroutine(timmer());
        }
    }
    private void DisbaelWin()
        {
        if (m_hudController != null)
        {
            
            this.transform.parent = null;
            m_videoPlayer.Stop();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            m_player.GetComponentInParent<PlayerInput>().SwitchCurrentActionMap("Player");
        }
    }

    private IEnumerator timmer()
    {
        yield return new WaitForSeconds(205f);
        DisbaelWin();
    }
}
