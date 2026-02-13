using System.Collections;
using UnityEngine;

public class Vines : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("How long the vines will burn")]
    [SerializeField] private float fireDuration;
    [SerializeField] private float waitToSpreadTime;

    [Header("References")]
    [SerializeField] private VinesAudio vinesAudio;
    [Tooltip("The Particle System Fire Effect")]
    [SerializeField] private ParticleSystem vinesOnFireEffect;
    [Tooltip("The Vines that is being Destroyed")]
    [SerializeField] private GameObject vinesToDestroy;
    [SerializeField] private SphereCollider fireSpreadCollider;

    public void InitiateFire()
    {
        StartCoroutine(StartVinesFire());
    }

    private IEnumerator StartVinesFire()
    {
        vinesOnFireEffect.Play();

        vinesAudio.StartBurning();

        yield return new WaitForSeconds(waitToSpreadTime);

        fireSpreadCollider.enabled = true;

        yield return new WaitForSeconds(fireDuration);

        vinesOnFireEffect.Stop();

        vinesAudio.StopBurning();

        Destroy(vinesToDestroy);
    }

}
