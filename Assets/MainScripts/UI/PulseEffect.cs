using UnityEngine;
using TMPro; 

public class PulseEffect : MonoBehaviour
{
    [SerializeField] private float m_speed = 2.0f;
    [SerializeField] private float m_minScale = 1.0f;
    [SerializeField] private float m_maxScale = 1.1f;
    [SerializeField] private float m_minAlpha = 0.5f;
    [SerializeField] private float m_maxAlpha = 1.0f;

    private CanvasGroup m_canvasGroup;
    private TextMeshProUGUI m_textMesh;  

    private void Start()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        UpdatePulse();
    }

    private void UpdatePulse()
    {
        float wave = (Mathf.Sin(Time.time * m_speed) + 1f) / 2f;

        float scale = Mathf.Lerp(m_minScale, m_maxScale, wave);
        transform.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(m_minAlpha, m_maxAlpha, wave);
        
        if (m_canvasGroup != null)
        {
            m_canvasGroup.alpha = alpha;
        }
        else if (m_textMesh != null)
        {
            Color c = m_textMesh.color;
            c.a = alpha;
            m_textMesh.color = c;
        }
    }
}

