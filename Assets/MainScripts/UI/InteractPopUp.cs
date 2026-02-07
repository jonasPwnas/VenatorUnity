using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Player;

public class InteractPopUp : MonoBehaviour
{
    [SerializeField] private GameObject pickUpText;
    private GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        pickUpText = GameObject.Find("InteractionHint");
    }

    private void Start()
    {
        pickUpText.SetActive(false);
    }


    private void OnTriggerEnter(Collider hit) 
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            pickUpText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            pickUpText.SetActive(false);
        }
    }

}
