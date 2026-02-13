using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Runtime.CompilerServices;

public class FallingBridge : ParentElementUser
{
    [Header("Bridge Falling Settings")]
    [SerializeField] private GameObject fallingBridge;
    [SerializeField] private Transform endPosition;
    [SerializeField] private float fallingDuration;

    [Header("Bridge Shake Settings")]
    [SerializeField] private float shakeDuration;
    [SerializeField] private float shakeStrength;

    [Header("Bridge Chain Settings")]
    [SerializeField] private Rigidbody[] chainRigidbodies;
    [SerializeField] private float chainsKinematicTimer;

    [Header("VFX")]
    [SerializeField] private ParticleSystem onImpactSmoke;

    [Header("Element Decal")]
    [SerializeField] private GameObject elementDecal;

    private FallingBridgeAudio m_fallingBridgeAudio;
    private bool m_isFalling = false;

    private void Awake()
    {
        m_fallingBridgeAudio = GetComponent<FallingBridgeAudio>();
    }


    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        if (m_isFalling)
            return;

        m_isFalling = true;
        
        if(elementType == ElementTypes.ElementType.Air)
        {
            m_fallingBridgeAudio.MovingSfx();
            BridgeFalling();
            StartCoroutine(ChainFalling());

            StartCoroutine(BridgeShake());
        }
    }

    private void BridgeFalling()
    {
        fallingBridge.transform.DORotate(endPosition.rotation.eulerAngles, fallingDuration);
        fallingBridge.transform.DOMoveY(endPosition.position.y, fallingDuration);
        elementDecal.SetActive(false);
    }

    private IEnumerator ChainFalling()
    {
        for(int i = 0; i < chainRigidbodies.Length; i++)
        {
            chainRigidbodies[i].isKinematic = false;
        }

        yield return new WaitForSeconds(chainsKinematicTimer);

        for (int i = 0; i < chainRigidbodies.Length; i++)
        {
            chainRigidbodies[i].isKinematic = true;
        }
    }

    private IEnumerator BridgeShake()
    {
        yield return new WaitForSeconds(fallingDuration);

        onImpactSmoke.Play();

        fallingBridge.transform.DOShakeRotation(shakeStrength, shakeDuration);
    }

}
