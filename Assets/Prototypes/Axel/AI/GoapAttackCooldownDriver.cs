using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GoapAttackCooldownDriver : MonoBehaviour
{
    [Tooltip("If empty, auto-finds GoapWorldStateBuilder in parents.")]
    [SerializeField] private GoapWorldStateBuilder m_worldStateBuilder;

    [Header("Cooldown Settings")]
    [Range(0f, 5f)]
    [SerializeField] private float m_lightAttackCooldownSeconds = 1.0f;

    [Range(0f, 8f)]
    [SerializeField] private float m_heavyAttackCooldownSeconds = 1.8f;

    [Range(0f, 8f)]
    [SerializeField] private float m_castCooldownSeconds = 2.0f;

    [Header("Debug (Read Only)")]
    [SerializeField] private bool m_isCoolingDown;
    [SerializeField] private float m_cooldownEndTime;

    private Coroutine m_cooldownRoutine;

    private void Awake()
    {
        AutoGetReferencesIfMissing();
        SetReady(true);
    }

    private void OnValidate()
    {
        AutoGetReferencesIfMissing();
    }

    public void StartLightAttackCooldown() => StartCooldown(m_lightAttackCooldownSeconds);
    public void StartHeavyAttackCooldown() => StartCooldown(m_heavyAttackCooldownSeconds);
    public void StartCastCooldown() => StartCooldown(m_castCooldownSeconds);

    public void CancelCooldownAndReady()
    {
        if (m_cooldownRoutine != null)
            StopCoroutine(m_cooldownRoutine);

        m_cooldownRoutine = null;
        m_isCoolingDown = false;
        m_cooldownEndTime = 0f;

        SetReady(true);
    }

    private void StartCooldown(float seconds)
    {
        if (m_worldStateBuilder == null)
            return;

        if (m_cooldownRoutine != null)
            StopCoroutine(m_cooldownRoutine);

        m_cooldownRoutine = StartCoroutine(CooldownRoutine(Mathf.Max(0f, seconds)));
    }

    private IEnumerator CooldownRoutine(float seconds)
    {
        m_isCoolingDown = true;
        m_cooldownEndTime = Time.time + seconds;

        SetReady(false);

        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);

        SetReady(true);

        m_isCoolingDown = false;
        m_cooldownEndTime = 0f;
        m_cooldownRoutine = null;
    }

    private void SetReady(bool isReady)
    {
        if (m_worldStateBuilder == null)
            return;

        m_worldStateBuilder.SetInt(GoapStateKey.AttackCooldownReady, isReady ? 1 : 0);
    }

    private void AutoGetReferencesIfMissing()
    {
        if (m_worldStateBuilder == null)
            m_worldStateBuilder = GetComponentInParent<GoapWorldStateBuilder>();
    }
}
