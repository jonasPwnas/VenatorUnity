using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float m_shakeDuration = 0.5f;
    [SerializeField] private AnimationCurve m_curve;

    [HideInInspector]public bool m_allowShake = true;

    private void Awake()
    {
        if (m_curve == null)
        {
            m_curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        }

        m_allowShake = PlayerPrefs.GetInt("CameraShake", 1) == 1;
    }
    public void OnAttacked()
        {
        if (m_allowShake)
        {
            StartCoroutine(Shake());
        }
    }

    private IEnumerator Shake()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < m_shakeDuration)
        {
            float strength = m_curve.Evaluate(elapsed / m_shakeDuration);
            transform.localPosition = originalPos + Random.insideUnitSphere * strength;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
