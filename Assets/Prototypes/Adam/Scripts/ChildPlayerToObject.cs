using System;
using Player;
using UnityEngine;

public class ChildPlayerToObject : MonoBehaviour
{
    [SerializeField] GameObject parentObject;

    private float playerHeightOffset = 0.2f;
    private PlayerController m_playerController;
    private bool m_playerInsideTrigger;

    private void Awake()
    {
        m_playerController = FindAnyObjectByType<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //m_playerController.transform.SetParent(parentObject.transform);
        m_playerInsideTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        //m_playerController.transform.SetParent(null);
        m_playerInsideTrigger = false;
    }

    private void LateUpdate()
    {
        if(m_playerInsideTrigger)
        {
            Vector3 newVelocity = Vector3.zero;
            
            newVelocity.y = parentObject.transform.position.y + playerHeightOffset -
                          m_playerController.transform.position.y;
            
            m_playerController.AddParentVelocity(newVelocity);
        }
    }
}
