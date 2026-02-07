using Characters;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyKnightCombatDriver : MonoBehaviour, ICombatDriver
{
    [SerializeField] private EnemyKnightRootMotionController m_motor;

    private void Awake()
    {
        if (m_motor == null)
            m_motor = GetComponent<EnemyKnightRootMotionController>();
    }

    public bool IsBusy => m_motor != null && m_motor.IsBusy;
    public bool IsBlocking => m_motor != null && m_motor.IsBlocking;
    public bool IsComboWindowOpen => m_motor != null && m_motor.IsComboWindowOpen;

    public void SetLockOn(bool isLockedOn, Transform target)
    {
        if (m_motor == null) return;
        m_motor.SetLockOnTarget(isLockedOn ? target : null);
    }

    public void SetMoveIntent(Vector2 move, bool strafeMode)
    {
        // optional – your movement is NavMesh driven
    }

    public bool TryLightAttack()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoLightAttack();
        return true;
    }

    public bool TryHeavyAttack()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoStrongAttack();
        return true;
    }

    public bool TryRoll()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoRoll();
        return true;
    }

    public void SetBlocking(bool isBlocking)
    {
        if (m_motor == null) return;
        m_motor.SetBlocking(isBlocking);
    }

    public bool TryCastFireAoE()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoFireAoE();
        return true;
    }

    public bool TryCastWaterHeal()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoWaterHeal();
        return true;
    }

    public bool TryCastWindPush()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.DoWindPush();
        return true;
    }

    public bool TryStartCharging()
    {
        if (m_motor == null || IsBusy) return false;
        m_motor.StartCharging();
        return true;
    }

    public void StopCharging()
    {
        if (m_motor == null) return;
        m_motor.StopCharging();
    }
}
