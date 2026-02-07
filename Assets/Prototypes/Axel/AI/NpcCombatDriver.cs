using Characters;
using UnityEngine;

[DisallowMultipleComponent]
public class NpcCombatDriver : MonoBehaviour, ICombatDriver
{
    [SerializeField] private NpcRootMotionController m_motor;

    private void Awake()
    {
        if (m_motor == null)
            m_motor = GetComponent<NpcRootMotionController>();
    }

    public bool IsBusy => m_motor != null && m_motor.IsBusy;
    public bool IsBlocking => m_motor != null && m_motor.IsBlocking;
    public bool IsComboWindowOpen => m_motor != null && m_motor.IsComboWindowOpen;

    public void SetLockOn(bool isLockedOn, Transform target)
    {
        if (m_motor == null)
            return;

        m_motor.SetLockOnTarget(isLockedOn ? target : null);
    }

    public void SetMoveIntent(Vector2 move, bool strafeMode)
    {
        if (m_motor == null)
            return;

        m_motor.SetMoveIntent(move, strafeMode);
    }

    public bool TryLightAttack() => m_motor != null && m_motor.TryLightAttack();
    public bool TryHeavyAttack() => m_motor != null && m_motor.TryHeavyAttack();
    public bool TryRoll() => m_motor != null && m_motor.TryRoll();

    public void SetBlocking(bool isBlocking)
    {
        if (m_motor == null)
            return;

        m_motor.SetBlocking(isBlocking);
    }

    public bool TryCastFireAoE() => m_motor != null && m_motor.TryCastFireAoE();
    public bool TryCastWaterHeal() => m_motor != null && m_motor.TryCastWaterHeal();
    public bool TryCastWindPush() => m_motor != null && m_motor.TryCastWindPush();

    public bool TryStartCharging() => m_motor != null && m_motor.TryStartCharging();
    public void StopCharging()
    {
        if (m_motor == null)
            return;

        m_motor.StopCharging();
    }
}
