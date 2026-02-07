using System.Collections;
using UnityEngine;

public class Barrel : ParentElementUser
{
    [Header("References")]
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

    private LockOnTargeting m_lockOnTargeting;
    
    private void Start()
    {
        m_lockOnTargeting = GetComponent<LockOnTargeting>();
        barrelOnFireParticleEffect.Stop();
        explosionParticleEffect.Stop();
    }
    
    private void Update()
    {
        if (barrelExplodeOnImpact.barrelImpact)
        {
            StartCoroutine(BarrelImpactExplosion());
        }
    }

    public override void OnActivated(float damageAmount, ElementTypes.ElementType elementType)
    {
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
        StartCoroutine(BarrelExplosion(timeUntilExplode));
    }

    private void ExplosionStop()
    {
        if (barrelOnFireParticleEffect.isPlaying)
            barrelOnFireParticleEffect.Stop();

        StopAllCoroutines();
    }
    
    private IEnumerator BarrelExplosion(float timeUntilExplode)
    {
        if (barrelOnFireParticleEffect.isStopped)
            barrelOnFireParticleEffect.Play();

        yield return new WaitForSeconds(timeUntilExplode);

        if (barrelOnFireParticleEffect.isPlaying)
            barrelOnFireParticleEffect.Stop();

        if (explosionParticleEffect.isStopped)
            explosionParticleEffect.Play();

        explosionRadius.enabled = true;
        barrelObj.enabled = false;
        yield return new WaitForSeconds(1.5f);

        if (explosionParticleEffect.isPlaying)
            explosionParticleEffect.Stop();

        Destroy(barrel);
    }

    #endregion

    #region Barrel Impact Explosion
    private IEnumerator ExplodeOnImpactActivation()
    {
        m_lockOnTargeting.StopLockOnWhenPhysics();
        
        yield return new WaitForSeconds(0.2f);

        if (!barrel.GetComponent<Rigidbody>().isKinematic)
        {
            explodeOnImpact.enabled = true;
        }
    }   

    private IEnumerator BarrelImpactExplosion()
    {
        if (explosionParticleEffect.isStopped)
            explosionParticleEffect.Play();
        
        explosionRadius.enabled = true;
        barrelObj.enabled = false;
        
        yield return new WaitForSeconds(1.5f);
        
        if (explosionParticleEffect.isPlaying)
            explosionParticleEffect.Stop();
        
        Destroy(barrel);
        
        barrelExplodeOnImpact.barrelImpact = false;
    }

    #endregion
}
