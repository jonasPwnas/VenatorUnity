using UnityEngine;

public interface ILockOnAble
{
    public Transform GetLockOnTarget();
    public void StopBeingLockedOn();
}
