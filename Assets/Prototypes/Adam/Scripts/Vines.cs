using System.Collections;
using UnityEngine;

public class Vines : MonoBehaviour
{
    [Header("Variables")]
    [Tooltip("How long the vines will burn")]
    [SerializeField] float fireDuration;

    [Header("References")]
    [Tooltip("The Particle System Fire Effect")]
    [SerializeField] ParticleSystem vinesOnFireEffect;
    [Tooltip("The Vines that is being Destroyed")]
    [SerializeField] GameObject vinesToDestroy;
    // Can later be changed to an animation that plays "detroying the vines"
    // Maybe add something that spawns when the vines are destroyed

    [Header("Used by Other Scripts")]
    [Tooltip("Used in 'ElementTriggerEvent'")]
    public bool fireActive;

    private void Start()
    {
        vinesOnFireEffect.Stop();
    }

    // remove and put it into the "damage" interface
    private void Update()
    {
        if (fireActive)
        {
            StartCoroutine(StartVinesFire(fireDuration));
        }
    }

    // Uses the 'fireDuration' float to set how long the fire will be active before destroying the vines
    private IEnumerator StartVinesFire(float fireDuration)
    {
        if(vinesOnFireEffect.isStopped)
            vinesOnFireEffect.Play();

        yield return new WaitForSeconds(fireDuration);

        if(vinesOnFireEffect.isPlaying)
            vinesOnFireEffect.Stop();

        Destroy(vinesToDestroy);

        // Spawn what will be there after the vines has burnt out.

        fireActive = false;
    }

}
