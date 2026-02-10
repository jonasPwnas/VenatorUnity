using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace TargetSystem
{
    public class TargetLockIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private List<ElementOption> m_elements;
        [SerializeField] private float m_rotationSpeed = 20f;
        [SerializeField] private Vector3 m_worldOffset = Vector3.up * 1.5f;
        [SerializeField] private float m_fadeDuration = 0.2f;
        [Range(0f, 1f)]
        [SerializeField] private float m_maxAlpha = 1f;

        [Header("UI Refs")]
        [SerializeField] private Image m_lockOnRing;
        [SerializeField] private Image m_vulnerabilityIcon;
        
        [System.Serializable]
        public struct ElementOption 
        { 
            public string name; 
            public Color color; 
            public Sprite icon; 
        }

        private CanvasGroup m_canvasGroup;
        private RectTransform m_rectTransform;
        private Camera m_mainCamera;
        
        private int m_currentIndex = 0;
        private bool m_isVisible = false;
        private Coroutine m_fadeCoroutine;

        private void Awake()
        {
            m_mainCamera = Camera.main;
            m_rectTransform = GetComponent<RectTransform>();
            m_canvasGroup = GetComponent<CanvasGroup>();
            // Ensure CanvasGroup exists for alpha control
            if (m_canvasGroup == null) m_canvasGroup = gameObject.AddComponent<CanvasGroup>();

            m_canvasGroup.alpha = 0f; // Start hidden
            m_rectTransform.localScale = Vector3.one * 0.5f; // Start small
        }

        // Auto-fill for convenience
        private void Reset()
        {
             m_elements = new List<ElementOption>
            {
                new ElementOption { name = "Water", color = Color.blue },
                new ElementOption { name = "Air", color = Color.white },
                new ElementOption { name = "Fire", color = Color.red }
            };
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ToggleLock();
            }

            if (m_isVisible || m_canvasGroup.alpha > 0.01f)
            {
                if (m_lockOnRing) m_lockOnRing.transform.Rotate(Vector3.forward, m_rotationSpeed * Time.deltaTime);
            }
        }

        private void ToggleLock()
        {
            m_isVisible = !m_isVisible;

            if (m_fadeCoroutine != null) StopCoroutine(m_fadeCoroutine);
            m_fadeCoroutine = StartCoroutine(AnimateVisibility(m_isVisible));

            if (m_isVisible)
            {
                ApplyElement(m_currentIndex);
            }
            else
            {
                // Switch to next element logic 
                if (m_elements != null && m_elements.Count > 0) 
                    m_currentIndex = (m_currentIndex + 1) % m_elements.Count;
            }
        }

        private void ApplyElement(int index)
        {
            if (m_elements == null || index < 0 || index >= m_elements.Count) return;
            var opt = m_elements[index];
            
            if (m_vulnerabilityIcon) 
            {
                m_vulnerabilityIcon.sprite = opt.icon; 
                m_vulnerabilityIcon.color = opt.color; 
            }
            if (m_lockOnRing) m_lockOnRing.color = opt.color;
        }

        private System.Collections.IEnumerator AnimateVisibility(bool show)
        {
            float startAlpha = m_canvasGroup.alpha;
            float endAlpha = show ? m_maxAlpha : 0f; 
            
            Vector3 startScale = m_rectTransform.localScale;
            Vector3 endScale = show ? Vector3.one : Vector3.one * 0.5f;
            
            // Snap effect
            if (show && startAlpha < 0.1f) 
            { 
                m_rectTransform.localScale = Vector3.one * 1.5f; 
                startScale = m_rectTransform.localScale; 
            }

            float t = 0f;
            float duration = Mathf.Max(m_fadeDuration, 0.01f);

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float smooth = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // Cubic Ease Out

                m_canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, smooth);
                m_rectTransform.localScale = Vector3.Lerp(startScale, endScale, smooth);
                yield return null;
            }
            m_canvasGroup.alpha = endAlpha;
            m_rectTransform.localScale = endScale;
        }
    }
}
