using DG.Tweening;
using UnityEngine;

public class SourceVFXManager : MonoBehaviour
{
    [SerializeField] public ParticleSystem whileChargingVFX;
    [SerializeField] public GameObject particleEffectSourceObject;

    [SerializeField] private float endValue;
    [SerializeField] private float startValue;
    [SerializeField] private float duration;

    private void Start()
    {
        whileChargingVFX.Play();
    }

    public void IncreaseSize()
    {
        particleEffectSourceObject.transform.DOScaleY(endValue, duration);
    }

    public void DecreaseSize()
    {
        particleEffectSourceObject.transform.DOScaleY(startValue, duration);
    }
}
