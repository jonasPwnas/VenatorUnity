using UnityEngine;
using DG.Tweening;
using System.Collections;

public class FallingBridge : ParentElementUser
{
    [SerializeField] private GameObject fallingBridge;

    [SerializeField] private Transform endPosition;

    [SerializeField] private float fallingDuration;

    [SerializeField] private Rigidbody[] chainRigidbodies;
    [SerializeField] private float chainsKinematicTimer;

    [Header("Bridge Shake Settings")]
    [SerializeField] private float duration;
    [SerializeField] private float strength;
    [SerializeField] private float vibrato;
    [SerializeField] private float randomness;


    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        if(elementType == ElementTypes.ElementType.Air)
        {
            Debug.Log("Rotating");
            fallingBridge.transform.DORotate(endPosition.rotation.eulerAngles, fallingDuration);
            fallingBridge.transform.DOMoveY(endPosition.position.y, fallingDuration);

            StartCoroutine(ChainFalling());

            //StartCoroutine(BridgeShake(fallingDuration));
        }
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

    //private IEnumerator BridgeShake(float fallingDuration)
    //{
    //    //yield return new WaitForSeconds(fallingDuration);

    //    //transform.DOShakePosition(duration, strength, vibrato, randomness);
    //}
}
