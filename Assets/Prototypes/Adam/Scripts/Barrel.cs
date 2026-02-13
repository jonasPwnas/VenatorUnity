using System.Collections;
using UnityEngine;

public class Barrel : ParentElementUser
{
    [Header("References")]
    [SerializeField] private BarrelAudio barrelAudio;
    [SerializeField] private GameObject barrel;
    [SerializeField] private Renderer barrelObj;
    [SerializeField] private BarrelExplodeOnImpact barrelExplodeOnImpact;
    
    [Header("Parameters")]
    [SerializeField] private SphereCollider explosionRadius;
    [SerializeField] private BoxCollider explodeOnImpact;

    [Header("Visuals")]
    [SerializeField] private ParticleSystem explosionParticleEffect;
    [SerializeField] private ParticleSystem barrelOnFireParticleEffect;

    [Header("Variables")]
    [SerializeField] private float timeUntilExplode = 3;

    [Header("Element Decal")]
    [SerializeField] private GameObject elementDecal;

    private LockOnTargeting m_lockOnTargeting;
    private bool m_explosionStarted = false;

    private void Start()
    {
        m_lockOnTargeting = GetComponent<LockOnTargeting>();
        barrelOnFireParticleEffect.Stop();
        explosionParticleEffect.Stop();
    }

    public void BarrelImpact()
    {
        StartCoroutine(BarrelImpactExplosion());
    }

    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
        barrelAudio.Attacked(elementType);

        switch (elementType)
        {
            case ElementTypes.ElementType.Fire:
                ExplosionStart();
                break;
            case ElementTypes.ElementType.Water:
                ExplosionStop();
                break;
            case ElementTypes.ElementType.Air:
                StartCoroutine(ExplodeOnImpactActivation());
                break;
        }
    }

    #region Barrel Timed Explosion
    private void ExplosionStart()
    {
        if (m_explosionStarted)
            return;

        m_explosionStarted = true;

        StartCoroutine(BarrelExplosion(timeUntilExplode));
    }

    private void ExplosionStop()
    {
        if (barrelOnFireParticleEffect.isPlaying)
            barrelOnFireParticleEffect.Stop();

        m_explosionStarted = false;

        barrelAudio.StopBurning();

        StopAllCoroutines();
    }

    private IEnumerator BarrelExplosion(float timeUntilExplode)
    {
        elementDecal.SetActive(false);

        barrelOnFireParticleEffect.Play();

        barrelAudio.StartBurning();

        yield return new WaitForSeconds(timeUntilExplode);

        barrelOnFireParticleEffect.Stop();

        explosionParticleEffect.Play();

        barrelAudio.StopBurning();

        barrelAudio.Explode();

        explosionRadius.enabled = true;
        barrelObj.enabled = false;
        yield return new WaitForSeconds(1.5f);

        explosionParticleEffect.Stop();

        Destroy(barrel);
    }

    #endregion

    #region Barrel Impact Explosion
    private IEnumerator ExplodeOnImpactActivation()
    {
        m_lockOnTargeting.StopLockOnWhenPhysics();

        barrelAudio.StopBurning();

        StopAllCoroutines();

        yield return new WaitForSeconds(0f);
    }

    private IEnumerator BarrelImpactExplosion()
    {
        if (explosionParticleEffect.isStopped)
            explosionParticleEffect.Play();
        elementDecal.SetActive(false);
        explosionRadius.enabled = true;

        barrelObj.enabled = false;

        barrelAudio.Explode();

        yield return new WaitForSeconds(1.5f);

        if (explosionParticleEffect.isPlaying)
            explosionParticleEffect.Stop();

        Destroy(barrel);
    }

    #endregion
}
