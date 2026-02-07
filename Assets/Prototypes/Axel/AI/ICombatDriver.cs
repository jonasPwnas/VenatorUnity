using UnityEngine;

public interface ICombatDriver
{
    bool IsBusy { get; }
    bool IsBlocking { get; }
    bool IsComboWindowOpen { get; }

    void SetLockOn(bool isLockedOn, Transform target);
    void SetMoveIntent(Vector2 move, bool strafeMode);

    bool TryLightAttack();
    bool TryHeavyAttack();
    bool TryRoll();

    void SetBlocking(bool isBlocking);

    bool TryCastFireAoE();
    bool TryCastWaterHeal();
    bool TryCastWindPush();

    bool TryStartCharging();
    void StopCharging();
}
