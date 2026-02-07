using UnityEngine;

namespace MainScripts
{
    public enum HitOutcome
    {
        Ignored,
        Hit,
        Blocked,
        PerfectBlocked,
        HitIframes
    }
    
    public readonly struct HitInfo
    {
        public readonly HitOutcome hitOutcome;
        public readonly Vector3 hitPoint;
        public readonly Vector3 incomingDirection;
        public readonly Transform attackerTransform;
        
        public HitInfo(
            HitOutcome hitOutcome,
            Vector3 hitPoint,
            Vector3 incomingDirection,
            Transform attackerTransform)
        {
            this.hitOutcome = hitOutcome;
            this.hitPoint = hitPoint;
            this.incomingDirection = incomingDirection;
            this.attackerTransform = attackerTransform;
        }
    }
}
