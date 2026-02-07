using UnityEngine;

public enum GoapStateKey
{
    Alerted = 0,
    TargetKnown = 1,
    TargetVisible = 2,
    TargetRange = 3,          // 0=None, 1=Far, 2=Chase, 3=Melee
    FacingTarget = 4,
    AttackCooldownReady = 5,
    TargetAttackingNow = 6,
    HasPathToTarget = 7,
    IsBlocking = 8,
    StrongPrimed = 9,
    PatrolStage = 10,
    SearchStage = 11, 
    SelfHealth01 = 12,       
    TargetHealth01 = 13,    
    IncomingThreat01 = 14,   
    DistanceBand = 15,      
    CombatMode = 16,      
    PressureState = 17, 
    CheeseScore01 = 18,       

    ActiveElement = 19,      
    FireCharges = 20,         
    WaterCharges = 21,        
    WindCharges = 22,         

    ChargePointKnown = 23, 
    AtChargePoint = 24,  
    PlayerInLockout = 25,   
    IsLockedOn = 26,           
    DesiredChargeElement = 27 // 0 none, 1 fire, 2 water, 3 air

}


public enum GoapCompareOp
{
    Equals,
    NotEquals,
    Greater,
    Less,
    IsTrue,
    IsFalse
}

public enum GoapEffectOp
{
    Set,
    Add,
    Subtract
}

public enum GoapActionPhase
{
    Started,
    Completed,
    Failed,
    Aborted
}
