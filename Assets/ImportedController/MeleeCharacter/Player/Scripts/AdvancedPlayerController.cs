using System;
using System.Collections;
using Input;
//using Tiny;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

//Add DOTween package to use this, is then used to move the camera to death position
//using DG.Tweening;

namespace Player
{ 
    public class AdvancedPlayerController : MonoBehaviour
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
        

        //Animator parameters
        private static readonly int DodgeRoll = Animator.StringToHash("DodgeRoll");//Add parameter to animator
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int Died = Animator.StringToHash("Died");
        private static readonly int Charging = Animator.StringToHash("Charging");
        private static readonly int Speed = Animator.StringToHash("MoveSpeed");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveZ = Animator.StringToHash("MoveZ");
        private static readonly int AttackMomentum = Animator.StringToHash("AttackMomentum");
        private static readonly int AttackType = Animator.StringToHash("AttackType");
        private static readonly int AttackStateTag = Animator.StringToHash("Attack");   // used ONLY with state.tagHash,  tag field on Animator states (select a state -> Inspector -> "Tag" dropdown).
        private static readonly int AttackTrigger = Animator.StringToHash("AttackTrigger"); // used for performing an attack 


        // NEW, New - Animator parameters
        private static readonly int AttackSteerDeg = Animator.StringToHash("AttackSteerDeg");
        private static readonly int ComboWindow = Animator.StringToHash("ComboWindow");



        // this onw might not be neccessary right now
        //private static readonly int CancelWindow = Animator.StringToHash("CancelWindow");


        //Editor editable
        [Tooltip("How fast we can jump after landing, in seconds")] [SerializeField] private float jumpFrequency = 0.4f;    // NOTE QUESTION: Can this be removed?
        [Tooltip("Downward force intensity")] [SerializeField] private float gravityIntensity = -6f;
        [Tooltip("How fast the player rotates")] [SerializeField] private float rotationSpeed = 10f;
        [Tooltip("The camera used by the player")] [SerializeField] private ThirdPersonCamera playerCamera;
        //[SerializeField] Trail trail;
        [SerializeField] private Transform deathCamTransform;
        [SerializeField] private PlayerElementCharger playerElementCharger;
        
        //Privates
        private Animator m_animator;
        private AdvancedPlayerInputHandler m_PlayerInputHandler;
        private CharacterController m_characterController;
        private Transform m_lockOnTarget;
        private Vector3 m_playerVelocity;
        private bool m_groundedPlayer;
        private bool m_allowJump = true;    // NOTE QUESTION: Can this be removed?
        private bool m_isDead;
        private bool m_canAttack = true;
                //private bool m_inputWindowOpen;  Remove the old event animation combo system 
        private float m_accumulatedAttackTime;
        private float m_attackTimer;
        private float m_targetMoveSpeed = 0.5f;
        private bool m_isStrafing = false;      // will be used for lock-on movement. (true = rotate toward a facing direction. fales = rotate toward movement, almost never walks backwards) 
        private float m_moveX;
        private float m_moveZ;
        private float m_jumpTimer;
        private bool m_isCharging = false;

        //NEW
        private bool m_comboConsumedThisWindow = false;
        private bool m_wasAttacking = false;
        private float m_maxAttackSteerDegPerSec = 240f; // try feeling what value feels best, something between 180–360 should do.
                                                        // Hard cap on how fast player can turn, frame-rate consistent


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
        }

        private void OnDisable()
        {
            PlayerElementCharger.OnFinishedCharging -= ChargingFinished;
            ThirdPersonCamera.OnStopLockOn -= StopStrafing;
        }

        private void ChargingFinished()
        {
            playerElementCharger.EnableCharger(false);
            m_animator.SetBool(Charging, false);
            OnChargeElement?.Invoke(m_isCharging);
        }

        private void StopStrafing()
        {
            EnableLockOnMode(false);
        }
        
        void Awake()
        {
            m_PlayerInputHandler = GetComponent<AdvancedPlayerInputHandler>();
            m_characterController = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            //trail.enabled = false;
        }
        
        void Update()
        {
            if (Time.timeScale == 0f)
            {
                return;
            }

            /*if(m_PlayerInputHandler.pauseAction.IsPressed())
                   OnGamePaused?.Invoke();*/

            Attack();
            DoChargeElement();
            Movement();
        }

        private void OnDeath()
        {
            //Move camera to nice cinematic location on death
            //StartCoroutine(MovecameraOnDeath());
            
            m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            m_animator.SetTrigger(Died);
            m_isDead = true;
            //trail.gameObject.SetActive(false);
        }
        
        private void DoChargeElement()
        {
            if (m_PlayerInputHandler.chargeElementAction.triggered && !m_isCharging)
            {
                m_isCharging = true;
                playerElementCharger.EnableCharger(true);
                OnChargeElement?.Invoke(m_isCharging);
                m_animator.SetBool(Charging, m_isCharging);
                return;
            }
            if (m_PlayerInputHandler.chargeElementAction.triggered && m_isCharging)
            {
                m_isCharging = false;
                ChargingFinished();
            }
        }
        
        void Attack()
        {
            // NEW 
            // Check if the character is capable of performing an attack, (currently inside the ComboWindow or can start a new attack) 
            GetAttackStateFlags(out bool attackStable, out bool attackRelevant);
            bool inComboWindow = m_animator.GetFloat(ComboWindow) > 0.5f;

            // Clear queued attack inputs when leaving attack state. Prevents any stale queued attack leaks into movement.
            if (m_wasAttacking && !attackRelevant)
            {
                m_PlayerInputHandler.ClearQueuedAttack();   // Clear stale queued attack from character input handler
                ClearAttacksFromAnimator();                 // Clear any leftover triggers and attack type id's in animator
            }

            m_wasAttacking = attackRelevant;


            // NOTE WARNING: Cooldown might block combo window if they happen before this timer?        //originally:  if (!m_canAttack)
            //               Might work as if (!attackRelevant && !m_canAttack) so QueuedPress doesn’t get consumed and
            //               lost during neutral cooldown but Combo chaining isn’t blocked by cooldown
            if (!attackRelevant && !m_canAttack)
            {
                // NOTE: Remember to look into if mixing Time.deltaTime and Time.time can cause any friction, for later. 
                m_accumulatedAttackTime += Time.deltaTime;
                if (m_accumulatedAttackTime >= m_attackTimer)
                {
                    m_canAttack = true;
                    m_attackTimer = 0f;

                    // ClearAttacksFromAnimator();  // should maybe not be here to risk clearing inputs wrongly in combo windows
                    // probably redundant with he current setup and if (m_wasAttacking && !attackRelevant) shouold be clearing this when needed! 
                }

                // if any attack input was stored during this attack-cooldown period, (and not in any state to make use of inComboWindow), ignore that input
                if (m_PlayerInputHandler.attackMode == AdvancedPlayerInputHandler.AttackInputMode.QueuedPress)
                {
                    // consume-and-drop any queued attack so it cannot fire later
                    m_PlayerInputHandler.TryGetAnyAttack(out _);

                    // Extra safety: ensure no value was left 
                    m_PlayerInputHandler.ClearQueuedAttack();
                }

                // Held mode: do nothing special; holding will attempt again next frame (easy mode)
                return;
            }


            // Re-arm check when combo window closes
            if (!inComboWindow)
                m_comboConsumedThisWindow = false;

            // Read attack input once (queued press or held mode). If there hasn't been any input, stop here 
            if (!m_PlayerInputHandler.TryGetAnyAttack(out int attackType) || attackType == 0)
                return;


            // Return early if attack is not possible right now, clear all stored attack inputs 
            if (attackRelevant && !inComboWindow)
            {
                // Only matters in QueuedPress (Held has nothing queued)
                m_PlayerInputHandler.ClearQueuedAttack();   // Strict timing on inputs, clear stale stored ones. (no late buffering) 
                return;
            }


            
            // NOTE: Combo chaining attacks is not blocked by the attack cooldown, animation curves has priority 
            // If chaining from an ongoing attack, consume the combo window after one successful input, guarded against consuming inputs twice
            if (attackStable && inComboWindow && !m_comboConsumedThisWindow)
            {
                FireAttack(attackType);
                m_comboConsumedThisWindow = true;   // attack input was consumed during this animation, prevent this triggering multiple times  
                return;
            }
            
            
            // When not already in an attack or in a transition into an attack
            if (!attackRelevant)
            {
                // start new attack only if not currently in any attack context
                FireAttack(attackType);
            }
        }

        private void FireAttack(int attackType)
        {
            // Tell animator which attack variant this trigger represents 
            m_animator.SetInteger(AttackType, attackType);
            m_animator.SetTrigger(AttackTrigger);

            // Debounce window (rate limiter)
            m_accumulatedAttackTime = Time.time;
            m_attackTimer = m_accumulatedAttackTime + m_attackInputFrequency;
            m_canAttack = false;
        }

        private void ClearAttacksFromAnimator()
        {
            m_animator.ResetTrigger(AttackTrigger);
            m_animator.SetInteger(AttackType, 0);
        }


        void Movement()
        {
            m_groundedPlayer = m_characterController.isGrounded;

            var currentState = m_animator.GetCurrentAnimatorStateInfo(0);
            GetAttackStateFlags(out bool attackStable, out bool attackRelevant);

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
            if (attackRelevant)
            {
                // Optional rotation help, only applies if AttackSteerDeg curve is present & > 0.
                TryApplyAttackSteering(worldDirNorm, inputMag);

                return; // IMPORTANT: still block normal locomotion logic & free rotation while attacking or transitioning into attack.
            }




            // if camera feels twitchy in idle state change to:  if (m_isStrafing && inputMag > 0.01f)
            // having rotation happen before the localDir gets sett might be more accurate for first frame of rotatin when strafing 
            if (m_isStrafing)
            {
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

            bool sprint = m_PlayerInputHandler.sprintAction.IsPressed(); 
            float moveTier = sprint ? 1f : 0.5f;      // set to sprint speed or to walk speed

            m_targetMoveSpeed= inputMag * moveTier;

            const float dampDir = 0.10f;
            const float dampSpeed = 0.06f;
            m_animator.SetFloat(MoveX, m_moveX, dampDir, Time.deltaTime);
            m_animator.SetFloat(MoveZ, m_moveZ, dampDir, Time.deltaTime);
            m_animator.SetFloat(Speed, m_targetMoveSpeed, dampSpeed, Time.deltaTime);




            
            if (m_groundedPlayer && m_playerVelocity.y < 0)
            {
                m_playerVelocity.y = 0f;
                m_animator.SetBool(IsGrounded, m_groundedPlayer);
            }
            
            
            //should we still use this?
            m_playerVelocity.y += gravityIntensity * Time.deltaTime;
            m_characterController.Move(m_playerVelocity * Time.deltaTime); 
            
            
            //if (currentState.tagHash != AttackStateTag)
            //{
            //    trail.enabled = false;
            //}
        }

        private void OnAnimatorMove() //Animator movement - root movement
        {
            var state = m_animator.GetCurrentAnimatorStateInfo(0);
            Vector3 velocity = m_animator.deltaPosition;
            
            gravityIntensity = -8f;
            
            if (state.tagHash == AttackStateTag)
            {
                velocity += transform.forward * m_animator.GetFloat(AttackMomentum) * Time.deltaTime;
                velocity.y += gravityIntensity * Time.deltaTime;
                //trail.enabled = true;
            }
            
            m_characterController.Move(velocity);
        }

        // should create more stability and no flickering states while transitioning.
        // Attack-state rules to remain active during:
        //      - Attack1 -> Attack2 blends
        //      - Movement -> Attack blends (so player don’t briefly regain locomotion/rotation)
        //      - Attack -> Movement blends (so player don’t get a brief free rotate burst on exit)
        //      - Trails and special effects wont flicker in transitions 
        private void GetAttackStateFlags(out bool attackStable, out bool attackRelevant)
        {
            var currentState = m_animator.GetCurrentAnimatorStateInfo(0);   // (the “from” state)      
            bool inTransition = m_animator.IsInTransition(0);             // when in transitioning, checking current state will only see: “Is the FROM-state tagged Attack (even if we are already leaving it)

            bool currentIsAttack = currentState.tagHash == AttackStateTag;

            bool nextIsAttack = false;
            if (inTransition)
            {
                var next = m_animator.GetNextAnimatorStateInfo(0);          // (the “to” state)
                nextIsAttack = next.tagHash == AttackStateTag;
            }


            // Fully committed inside an attack state (no blending)
            attackStable = currentIsAttack && !inTransition;

            // Attack is involved right now:
            // - either we are in an attack state already
            // - or we are blending into an attack state
            attackRelevant = currentIsAttack || (inTransition && nextIsAttack);
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

        
        IEnumerator MovecameraOnDeath()
        {
            float time = 1.2f;
            
            //Add DOTween package to use this
            //playerCamera.transform.DOMove(deathCamTransform.position, time);
            //playerCamera.transform.DORotate(deathCamTransform.eulerAngles, time, RotateMode.FastBeyond360);
            
            yield return new WaitForSecondsRealtime(time);
            print("After cam death move");
            OnDeathCamMoved?.Invoke();
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
            }
            else
            {
                m_isStrafing = enable;
                m_lockOnTarget = null;
            }
            
            OnLockOnModeUpdate?.Invoke(enable, m_lockOnTarget);
        }
        
        private void TrySetLockOnTarget()
        {
            if (m_lockOnTarget != null)
                return;
            
            Physics.SphereCast(transform.position + Vector3.up * 2f, 1.6f, playerCamera.transform.forward,
                out RaycastHit hit, playerCamera.lockOnDistance, playerCamera.lockOnLayerMask);
            
            if(hit.collider != null)
            {
                m_lockOnTarget = hit.transform;
                EnableLockOnMode(true);
            }
        }
    }
}