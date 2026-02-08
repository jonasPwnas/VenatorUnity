using System;
using System.Collections;
using Input;
using Tiny;
using UnityEngine;
using static PlayerStatus;

//Add DOTween package to use this, is then used to move the camera to death position
//using DG.Tweening;

namespace Player
{ 
    public class PlayerController : MonoBehaviour
    {
        //delegates and events
        public delegate void DeathCamMoved();
        public static event DeathCamMoved OnDeathCamMoved;
        
        public delegate void GamePaused();
        public static event GamePaused OnGamePaused;

        public delegate void ChargeElement(bool isCharging);
        public static event ChargeElement OnChargeElement;

        public delegate void LockOnModeUpdate(bool enable, Transform target);
        public static event LockOnModeUpdate OnLockOnModeUpdate;

        public delegate void CastSpell(ElementTypes.ElementType elementType);
        public static event CastSpell OnCastSpell;

        public delegate void Attacking();
        public static event Attacking OnAttacking;

        public delegate void Blocking(bool isBlocking);
        public static event Blocking OnBlocking;

        // Not Implemented Yet 
        public delegate void PerfectBlock(bool hasPerfectBlock);
        public static event PerfectBlock OnPerfectBlock;

        public delegate void ToggleIframes(bool hasIframes);
        public static event ToggleIframes OnToggleIframes;
        

        //Animator parameters
        private static readonly int Speed = Animator.StringToHash("MoveSpeed");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveZ = Animator.StringToHash("MoveZ");
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        
        private static readonly int DodgeStateTag = Animator.StringToHash("Dodge");   // NEW: Currently only on DodgeRoll, might need better setup in the future
        private static readonly int DodgeRollTrigger = Animator.StringToHash("DodgeRoll");//Add parameter to animator
        private static readonly int IFramesParam = Animator.StringToHash("IFrames");
        
        private static readonly int BlockStateTag = Animator.StringToHash("Block");
        private static readonly int BlockingBool = Animator.StringToHash("Blocking");
        private static readonly int BlockingParam = Animator.StringToHash("BlockingWindow");
        private static readonly int PerfectBlockingParam = Animator.StringToHash("PerfectBlockWindow");

        private static readonly int AttackStateTag = Animator.StringToHash("Attack");   // used ONLY with state.tagHash,  tag field on Animator states (select a state -> Inspector -> "Tag" dropdown).
        private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger"); // used for performing an attack 
        private static readonly int AttackType = Animator.StringToHash("AttackType");
        
        private static readonly int AttackMomentum = Animator.StringToHash("AttackMomentum");
        private static readonly int AttackSteerDeg = Animator.StringToHash("AttackSteerDeg");
        private static readonly int ComboWindow = Animator.StringToHash("ComboWindow");
        private static readonly int Died = Animator.StringToHash("Died");
        
        private static readonly int ChargeStateTag = Animator.StringToHash("Charging");
        private static readonly int Charging = Animator.StringToHash("Charging");
        //New, again! :D
        private static readonly int BigHit = Animator.StringToHash("BigHit");
        private static readonly int Hit = Animator.StringToHash("Hit");

        //Editor editable
        [Tooltip("How fast we can jump after landing, in seconds")] [SerializeField] private float jumpFrequency = 0.4f;
        [Tooltip("Downward force intensity")] [SerializeField] private float gravityIntensity = -6f;
        [Tooltip("How fast the player rotates")] [SerializeField] private float rotationSpeed = 10f;
        [Tooltip("The camera used by the player")] [SerializeField] private ThirdPersonCamera playerCamera;
        //[SerializeField] Trail trail;
        [SerializeField] private Transform lockOnCastPoint;
        [SerializeField] private PlayerElementCharger playerElementCharger;
        [SerializeField] private Transform lockOnIndicatorPosition;
        
        [Header("Stamina Costs")]
        [SerializeField] private float staminaRegenPerSecond = 12f;
        [SerializeField] private float overdraftPenalty = 1f;
        [SerializeField] private float dodgeCost = 20f;
        [SerializeField] private float lightAttackCost = 24f;
        [SerializeField] private float heavyAttackCost = 37f;
        [SerializeField] private float sprintCost = 0.5f;
        [SerializeField] private float blockingStaminaMultiplier = 0.25f;

        // Time To Live timers for input requests, reserve stamina calculation and refunds if actions were not performed in allocated time window   
        [Header("Stamina Requested TTL")]
        [SerializeField] private float m_dodgeRequestTTL = 0.35f;
        [SerializeField] private float m_attackRequestTTL = 0.35f;
        // private float m_sprintRequestTTL = 0.25f;
        
        [Header("Movement accelerations")]
        [SerializeField] private float accelDampTime = 0.1f;      // NEW
        [SerializeField] private float runToWalkDampTime = 0.5f;  // NEW
        [SerializeField] private float stopDampTime = 0.05f;      // NEW


        private struct FrameCombatFlags
        {
            public bool inStableDodge, dodgeRelevant;
            public bool inStableBlock, blockRelevant;
            public bool inStableAttack, attackRelevant;
            public bool inStableCharge, chargeRelevant; 
        }

        private FrameCombatFlags _f;

        //Privates
        private Animator m_animator;
        private PlayerStatus m_playerStatus;
        private PlayerInputHandler m_PlayerInputHandler;
        private CharacterController m_characterController;
        private PauseMenuController m_pauseManager;
        private PlayerObjectInteraction m_playerObjectInteraction = null;
        
        // NEW - for lock-on movement
        private Transform m_lockOnTarget;
        private Vector3 m_playerVelocity;

        // NEW - might delete and keep as mutable fields to makes it more obvious what frame’s data is being used, preventing stale stored values? 
        private bool m_isInTransition;
        private AnimatorStateInfo m_stateCurrent;
        private AnimatorStateInfo m_stateNext;

        // NEW - Temp torso layer for blocking
        private bool m_torsoInTransition;
        private AnimatorStateInfo m_torsoStateCurrent;
        private AnimatorStateInfo m_torsoStateNext;
        private int _torsoLayerIndex;

        private float m_targetMoveSpeed = 0.5f;
        private bool m_isStrafing = false;      // will be used for lock-on movement. (true = rotate toward a facing direction. fales = rotate toward movement, almost never walks backwards) 
        private float m_moveX;
        private float m_moveZ;

        private bool m_wasDodgingTruth = false;  
        private bool m_hasIFrames = false;

        private bool m_blockRequested = false;
        private bool m_wasBlockingTruth = false; 
        
        private int m_attackRequestedType;  // not really used right now, but good for something later (debug, analytics, HUD, cancel rules)
        private bool m_wasAttackingTruth = false;
        private bool m_canAttack = true;
        private float m_attackTimer;
        private bool m_comboConsumedThisWindow = false;
        private float m_maxAttackSteerDegPerSec = 240f;

        private bool m_updateCharacter = false;
        private bool m_isDead;
        private bool m_groundedPlayer;
        private float m_appliedGravityIntensity;
        private bool m_isCharging = false;
        private bool m_isCastingSpell = false;

        private float m_staminaPauseTimestamp;
        private float m_staminaMultiplier = 1f;

        // Stamina reservation tokens and TTL handling 
        private PlayerStatus.StaminaReservation m_dodgeReservation;
        private float m_dodgeRequestExpiresAt;
        private bool m_dodgeRequested = false;
        private PlayerStatus.StaminaReservation m_attackReservation;
        private float m_attackRequestExpiresAt;
        private bool m_attackRequested = false;
        private PlayerStatus.StaminaReservation m_comboAttackReservation;


        private bool m_doPickup = false;
        private float m_accumulatedAttackTime;  // REMOVE? 
        private bool m_canAddStamina = true;    // REMOVE? 
        private Coroutine m_pauseStaminaRoutine = null;    // REMOVE? old stamina 
        private bool m_allowJump = true;    // REMOVE? 
        private float m_jumpTimer;          // REMOVE? 
        


        //Read-only
        private readonly float m_attackInputFrequency = 0.25f;
        
        //Constants
        private const float DEADZONE = 0.15f;  // mainly to combat stick drift starting animations 
        
        //Publics
        [HideInInspector] public Vector3 moveVelocity;

        private void OnEnable()
        {
            PlayerElementCharger.OnFinishedCharging += ChargingFinished;
            ThirdPersonCamera.OnStopLockOn += StopStrafing;
            LockOnTargeting.OnStopLockOn += DisableLockOn;
            PlayerStatus.OnTookDamage += OnDamaged;
            PlayerStatus.OnPlayerDied += OnDeath;
            PlayerObjectInteraction.OnPickUp += SetDoPickupInsteadOfCharge;
        }

        private void OnDisable()
        {
            PlayerElementCharger.OnFinishedCharging -= ChargingFinished;
            ThirdPersonCamera.OnStopLockOn -= StopStrafing;
            LockOnTargeting.OnStopLockOn -= DisableLockOn;
            PlayerStatus.OnTookDamage -= OnDamaged;
            PlayerStatus.OnPlayerDied -= OnDeath;
            PlayerObjectInteraction.OnPickUp -= SetDoPickupInsteadOfCharge;
        }
        
        private void Awake()
        {
            if (WorldSettings.Instance == null)
            {
                Debug.LogError("WorldSettings not found???");
            }
            else
            {
                transform.position = WorldSettings.Instance.GetSpawnTransform().position;
            }
            
            m_PlayerInputHandler = GetComponent<PlayerInputHandler>();
            m_characterController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            m_playerStatus = GetComponent<PlayerStatus>();
            m_pauseManager = FindAnyObjectByType<PauseMenuController>();

            // IMPORTANT 
            // TEMP - remove after block has changed layer
            _torsoLayerIndex = m_animator.GetLayerIndex("Torso");
            if (_torsoLayerIndex < 0) Debug.LogError("Torso layer not found");
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            StartCoroutine(WaitToStartUpdate(0.3f));
        }

        void Update()
        {
            if (!m_updateCharacter) return;
            
            if (Time.timeScale == 0f) return;

            if (m_isDead)
                return;
            
            /*if(m_PlayerInputHandler.pauseAction.IsPressed())
                   OnGamePaused?.Invoke();*/

            // --- States Maintenance ---

            // Update recorded state info once per update, maybe also should do it in OnAnimatorMove as well ? 
            UpdateAnimStatesInfo();
            GetDodgingStateFlags(out _f.inStableDodge, out _f.dodgeRelevant);
            GetBlockingStateFlags(out _f.inStableBlock, out _f.blockRelevant);
            GetAttackStateFlags(out _f.inStableAttack, out  _f.attackRelevant);
            GetChargingStateFlags(out _f.inStableCharge, out _f.chargeRelevant);

            DodgeMaintenance();     // TTL + iFrames sync
            BlockMaintenance();     // stamina multiplier + OnBlocking truth events
            AttackMaintenance();    // TTL + cooldown unlock + cleanup

            // --- Input/request phase ---
            DoPause();
            Dodge();
            Block();
            DoCastSpell();
            Attack();
            DoChargeElement();
            Movement();
            UpdateStamina();
        }

        private void DoPause()
        {
            if (m_PlayerInputHandler.pauseAction.IsPressed())
            {
                m_pauseManager.PerformPause();
            }
        }
        
        
        private void UpdateStamina()
        {
            if (m_playerStatus.GetCurrentStaminaPerc() > 0.999f)
                return;

            if (Time.time < m_staminaPauseTimestamp)
                return;

            m_playerStatus.AddStamina(staminaRegenPerSecond * Time.deltaTime * m_staminaMultiplier);

            // NOTE: current regen is frame-rate dependent. Look int how something in this style would work: 
            //       m_playerStatus.AddStamina(staminaRegenPerSecond * Time.deltaTime * m_staminaMultiplier);
        }

        private void PauseStaminaRegen(float pauseSeconds = 1.2f)
        {
            // pause time does not stack but the longest one is the one that gets set, will prevent a shorter pause from overriding a longer one.
            m_staminaPauseTimestamp = Mathf.Max(m_staminaPauseTimestamp, Time.time + pauseSeconds);
        }

        private void CommitReservedStaminaCost(ref PlayerStatus.StaminaReservation staminaReservation, float pauseSeconds = 1.2f)
        {
            if (staminaReservation.requestedActionCost <= 0f) return;
            m_playerStatus.CommitReservedStamina(staminaReservation);
            
            // NOTE: This needs fixing, 
            if (staminaReservation.wouldOverdraw)
            {
                pauseSeconds += overdraftPenalty; 
                Debug.Log($"Stamina Penalty!  cost: {pauseSeconds}");
            }

            PauseStaminaRegen(pauseSeconds);
            staminaReservation = default;
        }

        private void ReleaseReservedStaminaCost(ref PlayerStatus.StaminaReservation staminaReservation)
        {
            if (staminaReservation.requestedActionCost <= 0f) return;
            m_playerStatus.ReleaseReservedStamina(staminaReservation); // clamp inside PlayerStatus
            staminaReservation = default; 
        }
        

        private void SetDoPickupInsteadOfCharge(bool doPickup, PlayerObjectInteraction objectInteract)
        {
            m_doPickup = doPickup;
            if(doPickup)
            {
                m_playerObjectInteraction = objectInteract;
            }
            else
            {
                m_playerObjectInteraction = null;
            }
        }
        
        private void ChargingFinished()
        {
            playerElementCharger.EnableCharger(false);
            m_animator.SetBool(Charging, false);
            OnChargeElement?.Invoke(m_isCharging);
        }

        
        
        private void DoCastSpell()
        {
            // --- Priority Locks for new input requests (After Maintenance) ---

            bool dodgeLocksSpells = _f.dodgeRelevant || m_dodgeRequested;
            if (dodgeLocksSpells)
                return;

            // NOTE: change later if attack state is not a hard blocker just in itself, but a part of a set of conditions 
            //      bool canCancelToAction = m_animator.GetFloat(EarlyOut) > 0.5f; // or CanDodge
            //      bool attackLocksAction = _f.isAttackRelevant && !canCancelToDodge;
            bool attackLocksSpells = _f.attackRelevant;
            if (attackLocksSpells)
                return;

            bool spellsLocksSpells = m_isCastingSpell;
            if (spellsLocksSpells)
                return;


            // --- Input handling (request) ---
            // 
            if (m_PlayerInputHandler.castSpellAction.triggered)
            {
                if (m_playerStatus.GetSourceElement() == ElementTypes.ElementType.NoElement)
                    return;
                
                if (m_playerStatus.GetCurrentElementAmount() == 0)
                    return;
                
                m_isCastingSpell = true;

                ElementTypes.ElementType coolElement = m_playerStatus.GetSourceElement();
                
                switch (coolElement)
                {
                    case ElementTypes.ElementType.Fire:
                        m_animator.SetTrigger("CastAoE");
                        m_playerStatus.RemoveElement(coolElement, 2);
                        break;
                    case ElementTypes.ElementType.Water:
                        m_animator.SetTrigger("Heal");
                        m_playerStatus.RemoveElement(coolElement, 1);
                        break;
                    case ElementTypes.ElementType.Air:
                        m_animator.SetTrigger("CastForward");
                        m_playerStatus.RemoveElement(coolElement, 1);
                        break;
                }
            }
        }

        public void CastFinished()//Called from anim event
        {
            m_isCastingSpell = false;
        }
        
        public void TriggerCast()
        {
            OnCastSpell?.Invoke(ElementTypes.ElementType.Air);
        }

        public void TriggerAoe()
        {
            OnCastSpell?.Invoke(ElementTypes.ElementType.Fire);
        }

        public void TriggerHeal()
        {
            OnCastSpell?.Invoke(ElementTypes.ElementType.Water);
        }

        private void OnDamaged(float damage, ElementTypes.ElementType elementType)
        {
            // NOTE: Change to this when curves exists 
            //bool blockTruth = _f.blockRelevant && (m_animator.GetFloat(BlockingParam) > 0.5f);
            //if (blockTruth) return;

            if (_f.blockRelevant || m_hasIFrames)
                return; //implement shield hit react pls and also stamina
            
            if (damage > 30)
            {
                m_animator.SetTrigger(BigHit);
            }
            else
            {
                m_animator.SetTrigger(Hit);
            }
        }

        private void OnDeath()
        {
            //Move camera to nice cinematic location on death
            //StartCoroutine(MovecameraOnDeath());
            
            m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            m_animator.SetTrigger(Died);
            m_isDead = true;
            enabled = false;
            //trail.gameObject.SetActive(false);
        }
        
        private void DodgeMaintenance()
        {
            bool dodgeTruth = _f.dodgeRelevant;
            bool enteredDodge = !m_wasDodgingTruth && dodgeTruth;

            // Confirm state was enterd succesfully: predicted stamina drain is locked as truth
            if (enteredDodge && m_dodgeRequested)
            {
                m_dodgeRequested = false;
                CommitReservedStaminaCost(ref m_dodgeReservation);
            }

            // If dodge was requested, but animator never transitioned: refund reserved stamina to player 
            if (m_dodgeRequested && Time.time >= m_dodgeRequestExpiresAt)
            {
                m_dodgeRequested = false;

                ReleaseReservedStaminaCost(ref m_dodgeReservation);
                m_animator.ResetTrigger(DodgeRollTrigger); // prevent late free dodge 
            }

            // Curve driven Iframes, only fire event on a change + inside dodge state. 
            bool iFramesNow = dodgeTruth && (m_animator.GetFloat(IFramesParam) > 0.5f);
            if (iFramesNow != m_hasIFrames)
            {
                // NOTE WARNING: if IFrames can be gained from elsewhere, this would turn of that as well!!

                // also cleans up if we exited dodge abruptly without resetting IFrames     
                m_hasIFrames = iFramesNow;
                OnToggleIframes?.Invoke(m_hasIFrames);
            }

            m_wasDodgingTruth = dodgeTruth;
        }

        private void Dodge()
        {
            // --- Priority Locks for new input requests (After Maintenance) ---

            // NOTE:
            //      Change later if attack state is not a hard blocker just in itself, but a part of a set of conditions 
            //      bool canCancelToAction = m_animator.GetFloat(EarlyOut) > 0.5f; // or CanDodge
            //      bool attackLocksAction = _f.isAttackRelevant && !canCancelToDodge;
            bool attackLocksDodge = _f.attackRelevant;
            bool spellsLocksDodge = m_isCastingSpell;

            if (attackLocksDodge || spellsLocksDodge)
                return;


            // --- Input handling (request) ---   

            // NOTE:
            //       There is currently no dodge follow up by another dodge limmiter,
            //       can chain dodge in infinity if animator alows.
            //       But animator setup should? be handling how ir feels  

            // Early outs after TTL and Maintenance is done 
            if (!m_PlayerInputHandler.dodgeAction.triggered)    // Input gate
                return;

            bool dodgeTruth = _f.dodgeRelevant;
            if (dodgeTruth || m_dodgeRequested)                 // Input-Spam / already in dodge-state gate
                return;

            if (!m_playerStatus.TryReserveStamina(dodgeCost, out m_dodgeReservation))   // Stamina gate
                return;

            // All conditions for a dodge has been met, stamina was paid by the check above but can be refunded
            m_dodgeRequested = true;
            m_dodgeRequestExpiresAt = Time.time + m_dodgeRequestTTL; 
            m_animator.SetTrigger(DodgeRollTrigger);
        }

        private void BlockMaintenance()
        {
            //bool blockTruth = _f.isBlockRelevant && (m_animator.GetFloat(BlockingParam) > 0.5f);
            bool blockTruth = _f.blockRelevant;     // will be fixed more precise by curves, right now "block = true" as soon as transition starts  

            if (blockTruth != m_wasBlockingTruth)
            {
                m_staminaMultiplier = blockTruth ? blockingStaminaMultiplier : 1f;  // mstch stamina behaviour to state 
                OnBlocking?.Invoke(blockTruth);                                     // send event only on truth change
                m_wasBlockingTruth = blockTruth;
            }
        }

        private void Block()
        {
            // --- Priority Locks for new input requests (After Maintenance) ---

            bool dodgeLocksDefense = _f.dodgeRelevant || m_dodgeRequested;
            // NOTE: change later if attack state is not a hard blocker just in itself, but a part of a set of conditions 
            //      bool canCancelToAction = m_animator.GetFloat(EarlyOut) > 0.5f; // or CanDodge
            //      bool attackLocksAction = _f.isAttackRelevant && !canCancelToDodge;
            bool attackLocksDefense = _f.attackRelevant;
            bool spellsLocksDefense = m_isCastingSpell;
            bool chargingLocksDefense = _f.chargeRelevant || m_isCharging;

            bool locked = (dodgeLocksDefense || attackLocksDefense || spellsLocksDefense || chargingLocksDefense);


            // --- Input handling (request) ---            

            bool wantsBlock = m_PlayerInputHandler.blockAction.IsPressed();
            bool shouldBlock = wantsBlock && !locked;

            // Sync request 
            if (shouldBlock != m_blockRequested)
            {
                m_blockRequested = shouldBlock;                  // Toggle request status
                m_animator.SetBool(BlockingBool, m_blockRequested); // Update animator with current request
            }
        }
        
        private void DoChargeElement()
        {
            // --- Priority Locks for new input requests (After Maintenance) ---

            bool dodgeLocksCharging = _f.dodgeRelevant || m_dodgeRequested;
            // NOTE: change later if attack state is not a hard blocker just in itself, but a part of a set of conditions 
            //      bool canCancelToAction = m_animator.GetFloat(EarlyOut) > 0.5f; // or CanDodge
            //      bool attackLocksAction = _f.isAttackRelevant && !canCancelToDodge;
            bool attackLocksCharging = _f.attackRelevant;
            bool spellsLocksCharging = m_isCastingSpell;

            bool locked = (dodgeLocksCharging || attackLocksCharging || spellsLocksCharging);


            // --- Input handling (request) --- 

            bool wantsCharge = m_PlayerInputHandler.chargeElementAction.IsPressed();
            bool shouldCharge = wantsCharge && !locked;

            
            if (shouldCharge && !m_isCharging && !m_doPickup)
            {
                m_isCharging = true;
                playerElementCharger.EnableCharger(true);
                OnChargeElement?.Invoke(m_isCharging);
                m_animator.SetBool(Charging, m_isCharging);
            }
            else if (!shouldCharge && m_isCharging)
            {
                m_isCharging = false;
                ChargingFinished();
            }
            else if (wantsCharge && m_doPickup && m_playerObjectInteraction != null)
            {
                m_playerObjectInteraction.PickUpElementCharges();
            }
        }


        private void AttackMaintenance()
        {
            bool attackRelevant = _f.attackRelevant;
            bool enteredAttack = !m_wasAttackingTruth && attackRelevant;
            bool leftAttackState = m_wasAttackingTruth && !attackRelevant;

            // Confirm: commit reserved stamina only when we actually entered an attack
            if (enteredAttack && m_attackRequested)
            {
                m_attackRequested = false;
                CommitReservedStaminaCost(ref m_attackReservation);
                m_attackRequestedType = 0;

                // Debounce window (rate limiter) only if attack actually happens
                m_attackTimer = Time.time + m_attackInputFrequency;
                m_canAttack = false;
            }

            // Expire: release reservation + clear animator params so it can't late fire
            if (m_attackRequested && Time.time >= m_attackRequestExpiresAt)
            {
                ReleaseReservedStaminaCost(ref m_attackReservation);
                m_attackRequestedType = 0;

                ClearAttacksFromAnimator(); // resets trigger + AttackType=0
                if (m_PlayerInputHandler.attackMode == PlayerInputHandler.AttackInputMode.QueuedPress)
                    m_PlayerInputHandler.ClearQueuedAttack();
                
                m_attackRequested = false;
            }


            // Leaving attack state cleanup: do NOT stomp pending TTL requests. Prevents any stale queued inputs or attack-triggers leaking into movement.
            if (leftAttackState && !m_attackRequested) 
            {
                m_PlayerInputHandler.ClearQueuedAttack();
                ClearAttacksFromAnimator();
            }

            m_wasAttackingTruth = attackRelevant;


            if (!attackRelevant && !m_canAttack)
            {
                // NOTE WARNING: Cooldown might block combo window if they happen before this timer?  
                //               Might work as if (!attackRelevant && !m_canAttack) so QueuedPress doesn’t get consumed and
                //               lost during neutral cooldown but Combo chaining isn’t blocked by cooldown

                // Cooldown tick only matters when not in attack context
                if (Time.time >= m_attackTimer)
                {
                    m_canAttack = true;
                    m_attackTimer = 0f;
                }

                // if any attack input was stored during this attack-cooldown period, (and not in any state to make use of inComboWindow), ignore that input
                if (m_PlayerInputHandler.attackMode == PlayerInputHandler.AttackInputMode.QueuedPress)
                {                    
                    m_PlayerInputHandler.TryGetAnyAttack(out _);    // consume-and-drop any queued attack so it cannot fire later
                    m_PlayerInputHandler.ClearQueuedAttack();       // Extra safety: ensure no value was left 
                }

                // NOTE: Held Mode - do nothing special, holding will attempt again next frame (easy mode)
            }
        }

        private void Attack()
        {
            // --- Priority Locks for new input requests (After Maintenance) ---

            // NOTE:
            //      Move this if combo attacks for some reson should not be blocked? 
            bool dodgeLocksAttacks = _f.dodgeRelevant || m_dodgeRequested;
            if (dodgeLocksAttacks)
                return;

            bool spellsLocksAttacks = m_isCastingSpell;
            if (spellsLocksAttacks)
                return;


            // --- Input handling (request) --- 

            // If in neutral cooldown, don't process input this frame
            if (!_f.attackRelevant && !m_canAttack)
                return;

            bool inComboWindow = m_animator.GetFloat(ComboWindow) > 0.5f;

            // Re-arm check when combo window closes
            if (!inComboWindow)
                m_comboConsumedThisWindow = false;

            // Read attack input once (queued press or held mode). If there hasn't been any input, stop here 
            if (!m_PlayerInputHandler.TryGetAnyAttack(out int attackType) || attackType == 0)
                return;

            // If already in an attack but not in combo window: strict timing (clear all stored attack inputs, no late buffering)
            if (_f.attackRelevant && !inComboWindow)
            {
                // Only matters in QueuedPress (Held has nothing queued)
                m_PlayerInputHandler.ClearQueuedAttack();
                return;
            }

            // NOTE: Combo chaining attacks is not blocked by the attack cooldown, animation curves has priority 
            //       If chaining from an ongoing attack, consume the combo window after one successful input, guarded against consuming inputs twice
            //       Might want to change to using attackRelevant instead of attackStable if inComboWindow ever overlaps with transition windows.
            //          - Allow combo buffering during transition once the combo window curve is open.
            //              - would maybe work to make combow window and ExitWindow to overlap a bit in a more stable fasion?

            // Combo chain
            if (_f.inStableAttack && inComboWindow && !m_comboConsumedThisWindow)
            {
                FireComboAttackImmediate(attackType);
                m_comboConsumedThisWindow = true;   // attack input was consumed during this animation, prevent this triggering multiple times  
                return;
            }

            // Neutral start
            if (!_f.attackRelevant)
            {
                RequestNeutralAttackWithTTL(attackType);
            }
        }

        private void RequestNeutralAttackWithTTL(int attackType)
        {
            float cost = GetAttackCost(attackType);
            if (cost <= 0f) return;

            if (m_attackRequested) return;

            if (!m_playerStatus.TryReserveStamina(cost, out m_attackReservation))
            {
                // Strict mode: clear queued input on failed attack
                if (m_PlayerInputHandler.attackMode == PlayerInputHandler.AttackInputMode.QueuedPress)
                    m_PlayerInputHandler.ClearQueuedAttack();
                return;
            }
            
            OnAttacking?.Invoke(); //for the ai, attack accepted (request created)

            // Tell animator which attack variant this trigger represents
            m_animator.SetInteger(AttackType, attackType);
            m_animator.SetTrigger(AttackTrigger);

            // TTL bookkeeping (only if request succeeded)
            m_attackRequested = true;
            m_attackRequestedType = attackType;
            m_attackRequestExpiresAt = Time.time + m_attackRequestTTL;
        }

        private void FireComboAttackImmediate(int attackType)
        {
            float cost = GetAttackCost(attackType);
            if (cost <= 0f) return;

            if (!m_playerStatus.TryReserveStamina(cost, out m_comboAttackReservation))
            {
                // Strict mode: clear queued input on failed attack
                if (m_PlayerInputHandler.attackMode == PlayerInputHandler.AttackInputMode.QueuedPress)
                    m_PlayerInputHandler.ClearQueuedAttack();
                return;
            }
            
            OnAttacking?.Invoke(); //for the ai, attack accepted (request created)

            // Tell animator which attack variant this trigger represents
            m_animator.SetInteger(AttackType, attackType);
            m_animator.SetTrigger(AttackTrigger);

            // Immediate spend for combo chain (request and confirm are effectively the same)
            // unlike normal attack I will asumme combo's will always be executed without needing to wait for "request + confirm" 
            CommitReservedStaminaCost(ref m_comboAttackReservation);

            // Debounce window (rate limiter)
            m_attackTimer = Time.time + m_attackInputFrequency;
            m_canAttack = false;
        }

        public void AnimEvent_Done()
        {
            //NO NO NOOOOOOO (we use this in the smart knight enemy)
        }
        public void AnimEvent_LookDone()
        {
            //NO NO NOOOOOOO (we use this in the smart knight enemy)
        }
        
        public void AddParentVelocity(Vector3 velocityToAdd)
        {
            transform.position += velocityToAdd;
        }
        
        void Movement()
        {
            m_groundedPlayer = m_characterController.isGrounded;

            if(m_PlayerInputHandler.lockOnAction.WasPressedThisFrame())
            {
                if(m_lockOnTarget == null)
                {
                    TrySetLockOnTarget();
                }
                else
                {
                    EnableLockOnMode(false);
                }
            }

            if (m_groundedPlayer && !m_allowJump)
            {
                m_jumpTimer += Time.deltaTime;
                if(m_jumpTimer >= jumpFrequency)
                {
                    m_allowJump = true;
                }
            }


            Vector2 input = m_PlayerInputHandler.moveAction.ReadValue<Vector2>();
            float inputMag = Mathf.Clamp01(input.magnitude);

            // dont read too small jitter values as real inputs 
            if (inputMag < DEADZONE) 
            {
                input = Vector2.zero;
                inputMag = 0f;
            }

            // camera relative direction on ground plane
            Vector3 camForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
            Vector3 camRight   = Vector3.ProjectOnPlane(playerCamera.transform.right, Vector3.up).normalized;

            // build world direction from input and normalize directions safely 
            Vector3 worldDir = camForward * input.y + camRight * input.x;
            Vector3 worldDirNorm = worldDir.sqrMagnitude > 1e-8f ? worldDir.normalized : Vector3.zero;



            // NEW
            // stop updating if we are in attack animation, but some clips will still allow some aiming / steering controll while attacking
            if (_f.attackRelevant)
            {
                // Optional rotation help, only applies if AttackSteerDeg curve is present & > 0.
                TryApplyAttackSteering(worldDirNorm, inputMag);

                return; // IMPORTANT: still block normal locomotion logic & free rotation while attacking or transitioning into attack.
            }

            

            // if camera feels twitchy in idle state change to:  if (m_isStrafing && inputMag > 0.01f)
            // having rotation happen before the localDir gets sett might be more accurate for first frame of rotatin when strafing 
            if (m_isStrafing && m_stateCurrent.tagHash != DodgeStateTag)   // NEW: Lock-on should now ignore dodge rolls 
            {
                bool inTransition = m_animator.IsInTransition(0);
                if (inTransition && m_animator.GetNextAnimatorStateInfo(0).tagHash == DodgeStateTag) // not yet in a dodge roll, but transitioning to one
                {
                    // turn player towards input direction
                    if (inputMag > 0f)
                    {
                        Quaternion targetInputRotation = Quaternion.LookRotation(worldDirNorm, Vector3.up);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetInputRotation, (rotationSpeed * 4) * Time.deltaTime); // still apply slerp rotation, but much faster than normally
                    }
                }


                Vector3 faceDir = GetStrafeFacingDir();
                Quaternion targetRotation = Quaternion.LookRotation(faceDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                if (inputMag > 0f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(worldDirNorm, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }

            // convert character local direction for animator 
            Vector3 localDir = transform.InverseTransformDirection(worldDirNorm);
            m_moveX = localDir.x;
            m_moveZ = localDir.z;

            bool sprint = false;


            // NOTE: Change to this when curves exists 
            //bool blockTruth = _f.blockRelevant && (m_animator.GetFloat(BlockingParam) > 0.5f);
            //if (blockTruth) return;

            if (!_f.blockRelevant)
                sprint = m_PlayerInputHandler.sprintAction.IsPressed(); 
            
            float moveTier = sprint ? 1f : 0.5f;      // set to sprint speed or to walk speed

            m_targetMoveSpeed= inputMag * moveTier;

            const float dampDir = 0.10f;
            //const float dampSpeed = 0.06f;
            m_animator.SetFloat(MoveX, m_moveX, dampDir, Time.deltaTime);
            m_animator.SetFloat(MoveZ, m_moveZ, dampDir, Time.deltaTime);
            //m_animator.SetFloat(Speed, m_targetMoveSpeed, dampSpeed, Time.deltaTime);

            // NEW - replaces two commented out lines above 
            float targetSpeed = m_targetMoveSpeed;
            float currentSpeed = m_animator.GetFloat(Speed);

            bool hasMoveInput = inputMag > 0f;

            // Pick different damping depending on what transition is happening
            float dampTime = !hasMoveInput ? stopDampTime :                                        // movement to stop sped deceleration 
                (targetSpeed < currentSpeed ? runToWalkDampTime : accelDampTime);   // decel vs accel

            m_animator.SetFloat(Speed, targetSpeed, dampTime, Time.deltaTime);

            if (m_groundedPlayer && m_playerVelocity.y < 0)
            {
                m_playerVelocity.y = 0f;
                m_animator.SetBool(IsGrounded, m_groundedPlayer);
            }
            
            if(m_characterController.isGrounded)
            {
                m_appliedGravityIntensity = 0f;
            }
            else
            {
                m_appliedGravityIntensity = gravityIntensity;
            }
            
            //should we still use this?
            m_playerVelocity.y += m_appliedGravityIntensity * Time.deltaTime;
            m_characterController.Move(m_playerVelocity * Time.deltaTime);
            
            //if (currentState.tagHash != AttackStateTag)
            //{
            //    trail.enabled = false;
            //}
        }


        private void OnAnimatorMove() //Animator movement - root movement
        {
            if (!m_updateCharacter) return;
            
            var state = m_animator.GetCurrentAnimatorStateInfo(0);
            Vector3 velocity = m_animator.deltaPosition;
            
            if (state.tagHash == AttackStateTag || state.tagHash == DodgeStateTag)   
            {
                velocity += transform.forward * m_animator.GetFloat(AttackMomentum) * Time.deltaTime;
                velocity.y += m_appliedGravityIntensity * Time.deltaTime;
                //trail.enabled = true;
            }
            
            m_characterController.Move(velocity);
        }

        private void TryApplyAttackSteering(Vector3 desiredWorldDirNorm, float inputMag)
        {
            if (inputMag <= DEADZONE) return;
            if (desiredWorldDirNorm.sqrMagnitude < 1e-8f) return;

            // If the curve doesn't exist / is not authored / is 0: Unity returns 0 -> and do nothing, ignoring movement input exactly as before.
            // Curve reading is caped 0-1 (and should on be set between those values anyways), and works as 0% - 100% of controll over character rotation
            float steer01 = Mathf.Clamp01(m_animator.GetFloat(AttackSteerDeg));
            if (steer01 <= 0.001f) return;

            // interpreting AttackSteerDeg curve as:  (control-deg %)/sec
            float turnRate = steer01 * m_maxAttackSteerDegPerSec;
            float maxStepThisFrame = turnRate * Time.deltaTime;

            Quaternion target = Quaternion.LookRotation(desiredWorldDirNorm, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxStepThisFrame);
        }
        
        
        private void UpdateAnimStatesInfo()
        {
            // Base layer (0)
            m_isInTransition = m_animator.IsInTransition(0);  // when in transitioning, checking current state will only see: “Is the FROM-state tagged Attack (even if we are already leaving it)
            m_stateCurrent = m_animator.GetCurrentAnimatorStateInfo(0); // (the curent state, or state being transitioned from)  
            m_stateNext = m_isInTransition ? m_animator.GetNextAnimatorStateInfo(0) : default;   // (the “to” state) Should always check m_inTransition before using!!! 

            // Torso layer
            m_torsoInTransition = m_animator.IsInTransition(_torsoLayerIndex);
            m_torsoStateCurrent = m_animator.GetCurrentAnimatorStateInfo(_torsoLayerIndex);
            m_torsoStateNext = m_torsoInTransition ? m_animator.GetNextAnimatorStateInfo(_torsoLayerIndex) : default;
        }

        private void GetDodgingStateFlags(out bool dodgeStable, out bool dodgeRelevant)
        {
            // Currently counting dodging as when we are in the dodge state or currently transitioning into it. 
            bool currentIsDodge = m_stateCurrent.tagHash == DodgeStateTag;

            bool nextIsDodge = false;
            if (m_isInTransition)
            {
                nextIsDodge = m_stateNext.tagHash == DodgeStateTag;
            }

            // Fully committed inside a dodge state (no blending)
            dodgeStable = currentIsDodge && !m_isInTransition;

            // Dodge is involved right now:
            dodgeRelevant = currentIsDodge || (m_isInTransition && nextIsDodge);
        }

        private void GetBlockingStateFlags(out bool blockStable, out bool blockRelevant)
        {
            // Currently counting dodging as when we are in the dodge state or currently transitioning into it. 
            bool currentIsBlock = m_torsoStateCurrent.tagHash == BlockStateTag;
            bool nextIsBlock = m_torsoInTransition && (m_torsoStateNext.tagHash == BlockStateTag);

            // Fully committed inside a dodge state (no blending)
            blockStable = currentIsBlock && !m_torsoInTransition;

            // Dodge is involved right now:
            blockRelevant = currentIsBlock || nextIsBlock;
        }

        private void GetChargingStateFlags(out bool chargeStable, out bool chargeRelevant)
        {
            // Currently counting dodging as when we are in the dodge state or currently transitioning into it. 
            bool currentIsCharge = m_stateCurrent.tagHash == ChargeStateTag;
            bool nextIsCharge = m_isInTransition && (m_stateNext.tagHash == ChargeStateTag);

            // Fully committed inside a dodge state (no blending)
            chargeStable = currentIsCharge && !m_isInTransition;

            // Dodge is involved right now:
            chargeRelevant = currentIsCharge || (m_isInTransition && nextIsCharge);
        }

        // should create more stability and no flickering states while transitioning.
        // Attack-state rules to remain active during:
        //      - Attack1 -> Attack2 blends
        //      - Movement -> Attack blends (so player don’t briefly regain locomotion/rotation)
        //      - Attack -> Movement blends (so player don’t get a brief free rotate burst on exit)
        //      - Trails and special effects wont flicker in transitions 
        private void GetAttackStateFlags(out bool attackStable, out bool attackRelevant)
        {      
            bool currentIsAttack = m_stateCurrent.tagHash == AttackStateTag;

            bool nextIsAttack = false;
            if (m_isInTransition)
            {
                nextIsAttack = m_stateNext.tagHash == AttackStateTag;
            }

            // Fully committed inside an attack state (no blending)
            attackStable = currentIsAttack && !m_isInTransition;

            // Attack is involved right now:
            // - either we are in an attack state already
            // - or we are blending into an attack state
            attackRelevant = currentIsAttack || (m_isInTransition && nextIsAttack);
        }

        private float GetAttackCost(int attackType)
        {
            return attackType == 1 ? lightAttackCost :
                   attackType == 2 ? heavyAttackCost :
                   0f;
        }

        private void ClearAttacksFromAnimator()
        {
            m_animator.ResetTrigger(AttackTrigger);
            m_animator.SetInteger(AttackType, 0);
        }

        
        // NOTE: camera is a placeholder for testing strafing,
        // later replace with something like (target.position - transform.position) projected onto plane.
        private Vector3 GetStrafeFacingDir()
        {
            
            Vector3 faceDir = Vector3.ProjectOnPlane(m_lockOnTarget.position - transform.position, Vector3.up); //add lock on target here
            if (faceDir.sqrMagnitude < 1e-6f) return transform.forward;

            return faceDir.normalized;
        }

        private void EnableLockOnMode(bool enable) //handle hud and other things for lock on mode here
        {
            if(enable)
            {
                m_isStrafing = enable;
                OnLockOnModeUpdate?.Invoke(enable, m_lockOnTarget);
            }
            else
            {
                m_lockOnTarget.GetComponent<ILockOnAble>().StopBeingLockedOn();
                m_isStrafing = enable;
                OnLockOnModeUpdate?.Invoke(enable, m_lockOnTarget);
                m_lockOnTarget = null;
            }
            
        }
        
        private void TrySetLockOnTarget()
        {
            if (m_lockOnTarget != null)
                return;
            
            Physics.SphereCast(lockOnCastPoint.position, 1.6f, lockOnCastPoint.forward,
                out RaycastHit hit, playerCamera.lockOnDistance, playerCamera.lockOnLayerMask);
            
            if(hit.collider != null)
            {
                m_lockOnTarget = hit.transform;
                EnableLockOnMode(true);
            }
        }

        public void DisableLockOn()
        {
            m_isStrafing = false;
            m_lockOnTarget = null;
            OnLockOnModeUpdate?.Invoke(false, m_lockOnTarget);
        }

        private void StopStrafing()
        {
            EnableLockOnMode(false);
        }

        IEnumerator WaitToStartUpdate(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            m_updateCharacter = true;
        }
        
    }
}