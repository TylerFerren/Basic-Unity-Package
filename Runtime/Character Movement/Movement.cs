using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System;

namespace Codesign
{
    [RequireComponent(typeof(CharacterController), typeof(Animator))]
    public class Movement : MonoBehaviour
    {
        #region Fields
        public CharacterController controller;
        public Camera cam;
        public Animator anim;
        public Status stanima;
        private StandardInputs input;
        [SerializeField, ShowIf("usePlayerInputComponent")] private PlayerInput playerInput;

        #region Settings
        [FoldoutGroup("Basic Movement"), SerializeField, Tooltip("Base Speed of standard movement")] private float standardSpeed = 5f;
        [FoldoutGroup("Basic Movement"), SerializeField] private float acceleration = 5;
        [FoldoutGroup("Basic Movement"), SerializeField] private float directionalAcceleration = 3;
        [FoldoutGroup("Basic Movement"), SerializeField] private AnimationCurve offAngleSpeedReduction = AnimationCurve.Constant(0, 180, 1);

        [FoldoutGroup("Sprint"), SerializeField, Tooltip("Top Speed of standard movement")] private float sprintSpeed = 9f;
        [FoldoutGroup("Sprint"), SerializeField] private bool sprintUsesStanima = false;
        [FoldoutGroup("Sprint"), SerializeField, ShowIf("sprintUsesStanima")] private float sprintStanimaCost = 3;

        [FoldoutGroup("Rotation"), SerializeField] private bool targetLock = false;
        [FoldoutGroup("Rotation"), SerializeField, Range(0.0f, 720f)] private float rotationSpeed = 180f;
        [FoldoutGroup("Rotation"), SerializeField, Range(0.0f, 720f)] private float airRotationSpeed = 150f;

        [FoldoutGroup("Jump"), SerializeField] private float jumpHeight = 3f;
        [FoldoutGroup("Jump"), SerializeField] private float jumpCount = 1f;
        [FoldoutGroup("Jump"), SerializeField, Tooltip("movement speed when not grounded")] private float airMoveSpeed = 3f;
        [FoldoutGroup("Jump"), SerializeField, Tooltip("adjusts amount of momentum lost when airborn")] private float airResitance = 1f;
        [FoldoutGroup("Jump"), SerializeField, Range(1f, 8f)] private float shortJumpMultipler = 2;
        [FoldoutGroup("Jump"), SerializeField, Range(1f, 8f)] private float fallMultipler = 1;
        [FoldoutGroup("Jump"), SerializeField] private Vector3 gravity = Physics.gravity;

        [FoldoutGroup("Crouch"), SerializeField] private float CrouchSpeed = 2;
        [FoldoutGroup("Crouch"), SerializeField] private float CrouchHeight = 1;

        [FoldoutGroup("Dodge"), SerializeField] private float dodgeDistance = 4;
        [FoldoutGroup("Dodge"), SerializeField] private float dodgeDelay = 0.2f;
        [FoldoutGroup("Dodge"), SerializeField] private float dodgeCooldown = 0.3f;
        [FoldoutGroup("Dodge"), SerializeField] private float dodgeSpeed = 3;
        [FoldoutGroup("Dodge"), SerializeField] private bool dodgeUsesStanima = false;
        [FoldoutGroup("Dodge"), SerializeField, ShowIf("dodgeUsesStanima")] private float dodgeStanimaCost = 10;
        [FoldoutGroup("Dodge"), SerializeField] private bool dodgeMustBeGrounded;

        [FoldoutGroup("Ground Detection"), SerializeField] private float groundedOffset = -0.05f;
        [FoldoutGroup("Ground Detection"), SerializeField] private float groundedRadius = 0.05f;
        [FoldoutGroup("Ground Detection"), SerializeField] private LayerMask groundLayers = 1 << 0;
        [FoldoutGroup("Ground Detection"), SerializeField] private bool moveWithGround;
        [FoldoutGroup("Ground Detection"), SerializeField] private float balance = 0.25f;

        [FoldoutGroup("Slope Movement"), SerializeField, Range(0, 90)] private float slopeLimit = 55;
        [FoldoutGroup("Slope Movement"), SerializeField, Range(0, 10)] private float slopeSpeedReduction = 1;
        [FoldoutGroup("Slope Movement"), SerializeField, Range(0, 90)] private float slideStart = 45f;
        [FoldoutGroup("Slope Movement"), SerializeField, Range(0, 10)] private float slideFriction = 0.3f;

        [FoldoutGroup("input"), SerializeField] private bool usePlayerInputComponent;
        [FoldoutGroup("input"), Button, ShowIf("usePlayerInputComponent")] public void SetupCaller() => SetupPlayerInputComponent();
        [FoldoutGroup("input"), SerializeField, HideIf("usePlayerInputComponent")] private bool useCustomInput;
        [FoldoutGroup("input"), SerializeField, ShowIf("useCustomInput")] private InputActionReference MoveInput;
        [FoldoutGroup("input"), SerializeField, ShowIf("useCustomInput")] private InputActionReference SprintInput;
        [FoldoutGroup("input"), SerializeField, ShowIf("useCustomInput")] private InputActionReference JumpInput;
        [FoldoutGroup("input"), SerializeField, ShowIf("useCustomInput")] private InputActionReference CrouchInput;
        [FoldoutGroup("input"), SerializeField, ShowIf("useCustomInput")] private InputActionReference DodgeInput;

        [FoldoutGroup("Animation"), SerializeField] private Animator AnimSpeed;
        #endregion

        #region Inputs
        private Vector2 inputDirection { get; set; }
        private bool sprinting { get; set; }
        #endregion

        #region modifiers
        public bool FreezeMovement { get; set; }
        public float SpeedModifier { get; set; } = 1;
        #endregion

        #region outputs
        public Vector3 Direction { get; private set; }
        public Vector3 TargetDirection { get; private set; }
        public float LastJumpHieght { get; set; } = 0;
        public float EnergyCost { get { return sprintStanimaCost; } set { sprintStanimaCost = value; } }
        public bool UsingStanima { get { return usingStanima; } set { usingStanima = value; } }
        #endregion

        #region local
        private float speed;
        private Vector3 relativeInput;
        private bool usingStanima = false;

        private float targetRotation;
        private float inputAngleChange;

        private bool isGrounded;

        private bool isCrouching;
        private float standingHeight;

        private bool isDodging;
        private Coroutine currentDodge;

        private bool isJumping;
        private float VerticalVelocity;
        private float currentJumpCount;
        private float fallForce;
        private float currentShortJumpMultipler;
        private float currentFallMultiplier;

        private Vector3 normal;
        private Vector3 slide;
        private Vector3 smoothSlide;
        private bool isSliding;

        private Vector3 GroundCurrent, GroundPrevious;
        private Transform Ground;
        private Vector3 BalancedMovement;
        #endregion

        #endregion

        #region Unity Event Methods

        public void Reset()
        {
            if (!controller) controller = GetComponent<CharacterController>();
            if (!anim) anim = GetComponent<Animator>();
            input = new StandardInputs();
        }

        public void OnValidate()
        {

        }

        public void OnEnable()
        {
            if (input == null) input = new StandardInputs();
            input.Enable();
            SetupInput();
        }

        public void OnDisable()
        {
            input.Disable();
        }

        public void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            if (!cam) cam = Camera.main;

            if (sprintUsesStanima && !stanima) stanima = GetComponent<Status>();

            slideStart = slideStart == 0 ? slopeLimit : slideStart;
            if (slopeLimit != 0)
                controller.slopeLimit = slopeLimit;

            if (controller) standingHeight = controller.height;
        }

        private Vector3 MovementVector;
        public void Update()
        {

            //check if player is grounded
            isGrounded = GroundedCheck();

            if (FreezeMovement) return;

            //Rotates the controller
            RotationCalc();

            //lateral movement vector based on direction and speed
            MovementVector = Time.deltaTime * SpeedCalc() * DirectionCalc();

            ////adds vertical velocity
            //MovementVector += JumpCalc();

            ////adds gravity
            //MovementVector += gravity * Time.deltaTime;

            ////Adjust speed on slopes
            //MovementVector += FindSlope();

            ////Moves with the transform it is currently standing on
            //MovementVector += GroundMovement();

            controller.Move(MovementVector);
        }
        #endregion

        #region MovementCalc
        private Vector3 DirectionCalc()
        {
            //gets a direction relative to the look vector
            relativeInput = Quaternion.Euler(0.0f, cam.transform.eulerAngles.y, 0.0f) * new Vector3(inputDirection.x, 0, inputDirection.y);

            //increases directional excelleration if sprinting
            //var directionMultiplier = sprinting ? directionalAcceleration * (sprintSpeed / standardSpeed) : directionalAcceleration;

            //smoothes out target direction
            //if controller is grounded, movement is smoothly projected on the plane of the ground normal
            if (Physics.SphereCast(gameObject.transform.position + Vector3.up * 0.2f, 0.1f, Vector3.down, out RaycastHit hit, 3) && isGrounded)
            {
                var groundRelativeDir = Vector3.ProjectOnPlane(relativeInput, hit.normal);
                TargetDirection = Vector3.Distance(TargetDirection, groundRelativeDir) > 0.01f ? Vector3.Lerp(TargetDirection, groundRelativeDir, Time.deltaTime * directionalAcceleration) : groundRelativeDir;
            }
            else
                TargetDirection = Vector3.Distance(TargetDirection, relativeInput) > 0.01f ? Vector3.Lerp(TargetDirection, relativeInput, Time.deltaTime * directionalAcceleration) : relativeInput;

            //smoothes out direction
            Direction = Vector3.Distance(Direction, TargetDirection) > 0.01f ? Vector3.Lerp(Direction, TargetDirection, Time.deltaTime * directionalAcceleration) : TargetDirection;

            return Direction;
        }

        //return a non-smoothed direction calculation
        private Vector3 DirectionCalcRaw()
        {
            //gets a direction relative to the look vector
            relativeInput = Quaternion.Euler(0.0f, cam.transform.eulerAngles.y, 0.0f) * new Vector3(inputDirection.x, 0, inputDirection.y);

            //if controller is grounded, movement is smoothly projected on the plane of the ground normal
            if (Physics.SphereCast(gameObject.transform.position + Vector3.up * 0.2f, 0.1f, Vector3.down, out RaycastHit hit, 3) && isGrounded)
            {
                var groundRelativeDir = Vector3.ProjectOnPlane(relativeInput, hit.normal);
                TargetDirection = groundRelativeDir;
            }
            else
                TargetDirection = relativeInput;

            Direction = TargetDirection;

            return Direction;
        }

        private float SpeedCalc()
        {
            //keeps momentum at the begining of a jump
            var airSpeed = speed > airMoveSpeed ? Mathf.Lerp(speed, airMoveSpeed, Time.deltaTime * airResitance) : airMoveSpeed;

            float targetSpeed;

            //Checks if the character is grounded, crouching, or sprinting input and sets speed accordingly
            if (inputDirection == Vector2.zero) targetSpeed = 0;
            else if (isGrounded)
            {
                if (isCrouching)
                {
                    targetSpeed = CrouchSpeed;
                }
                else if (sprinting)
                {
                    //checks if sprint uses stanima and if character has enough stanima
                    if (sprintUsesStanima && stanima.CurrentValue <= 0) targetSpeed = standardSpeed;
                    else targetSpeed = sprintSpeed;
                }
                else targetSpeed = standardSpeed;
            }
            else targetSpeed = airSpeed;

            //Starts using Energy if target speed is close to sprint speed
            if (sprintUsesStanima && MathF.Abs(targetSpeed - sprintSpeed) <= 1)
            {
                stanima.AdjustStatus(-sprintStanimaCost * Time.deltaTime);
            }

            //adjust speed by offAngleSpeedReduction - input direction vs facing direction
            var relativeFacingAngle = Vector3.SignedAngle(transform.forward, Direction, Vector3.up);
            targetSpeed *= offAngleSpeedReduction.Evaluate(relativeFacingAngle);

            targetSpeed *= SpeedModifier;

            //smooths speed towards target speed
            if (speed < targetSpeed - 0.1f || speed > targetSpeed + 0.1f)
                speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * acceleration);
            else
                speed = targetSpeed;

            //rounds to hundths place
            speed = Mathf.Round(speed * 100f) / 100f;

            return speed;
        }

        private void RotationCalc()
        {
            //chects if there is any directional input currently happening
            if (inputDirection != Vector2.zero)
            {
                //sets rotation speed bas on if character is grounded or not
                float rotSpeed = isGrounded ? rotationSpeed : airRotationSpeed;

                // set target rotation toward move direction or camera forward
                if (!targetLock) targetRotation = Mathf.Atan2(relativeInput.normalized.x, relativeInput.normalized.z) * Mathf.Rad2Deg;
                else targetRotation = cam.transform.eulerAngles.y;

                //gets a angle to rotate the character toward the target rotation
                float rotation = Mathf.MoveTowardsAngle(controller.transform.eulerAngles.y, targetRotation, rotSpeed * Time.deltaTime);

                // rotate to face input direction relative to camera position
                controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation, Quaternion.Euler(0.0f, rotation, 0.0f), rotSpeed * Time.deltaTime);

                //angle between current heading and input heading
                inputAngleChange = Vector3.SignedAngle(controller.transform.forward, inputDirection, Vector3.up) / 90;
            }
            else inputAngleChange = Mathf.Abs(inputAngleChange) > 0.05 ? Mathf.LerpAngle(inputAngleChange, 0, Time.deltaTime) : 0;
        }

        private Vector3 JumpCalc()
        {
            //gets current vertical Momentum
            VerticalVelocity = controller.velocity.y;

            //checks if charcter grounded but not trying to jump
            if (isGrounded && fallForce < -gravity.y)
            {
                fallForce = 0;
                currentJumpCount = 0;
            }
            else if (!isGrounded)
            {
                if (!isJumping && fallForce > -gravity.y / 2)
                {
                    currentShortJumpMultipler = Mathf.Lerp(currentShortJumpMultipler, shortJumpMultipler, Time.deltaTime * 2);
                }

                if (VerticalVelocity <= 0)
                {
                    currentFallMultiplier = Mathf.Lerp(currentFallMultiplier, fallMultipler, Time.deltaTime * 2);
                }

                fallForce += gravity.y * currentShortJumpMultipler * currentFallMultiplier * Time.deltaTime;
            }

            if (controller.transform.position.y > LastJumpHieght)
                LastJumpHieght = controller.transform.position.y;

            return new Vector3(0, fallForce * Time.deltaTime, 0);
        }

        private void Crouch()
        {
            if (isCrouching)
            {
                controller.height = CrouchHeight;
            }
            else controller.height = standingHeight;
        }

        public bool GroundedCheck()
        {
            var position = controller.transform.position;
            var spherePosition = new Vector3(position.x, position.y - ((controller.height / 2) - controller.radius) - groundedOffset, position.z);

            Collider[] colliders;
            if (controller.gameObject.layer == groundLayers)
                colliders = Physics.OverlapSphere(spherePosition, controller.radius + groundedRadius);
            else
                colliders = Physics.OverlapSphere(spherePosition, controller.radius + groundedRadius, ~controller.gameObject.layer, QueryTriggerInteraction.Ignore);

            foreach (var collider in colliders)
            {
                if (!collider.transform.IsChildOf(controller.transform))
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 FindSlope()
        {
            if (Physics.SphereCast(controller.transform.position + Vector3.up * 0.2f, 0.1f, Vector3.down, out RaycastHit hit, 3))
            {
                normal = hit.normal;

                var FacingAngle = Vector3.Angle(controller.transform.forward * -1, normal);
                var targetSpeedModifier = isGrounded ? Mathf.Pow(FacingAngle / 90, slopeSpeedReduction) : 1;

                SpeedModifier = Mathf.Lerp(SpeedModifier, targetSpeedModifier, Time.deltaTime);

                return Sliding();
            }

            return Vector3.zero;
        }

        private Vector3 Sliding()
        {
            slide = new Vector3((1f - normal.y) * normal.x, 0, (1f - normal.y) * normal.z);

            var slopeAngle = Vector3.Angle(normal, Vector3.up);

            isSliding = slopeAngle > slideStart ? true : false;

            var directionalSlide = slopeAngle > slideStart ? Vector3.ProjectOnPlane(slide, normal) : Vector3.zero;

            if (Vector3.Distance(smoothSlide, directionalSlide) > 0.1)
                smoothSlide = Vector3.Lerp(smoothSlide, directionalSlide, Time.deltaTime);
            else
                smoothSlide = directionalSlide;

            return smoothSlide * Time.deltaTime * (10 - slideFriction);
        }

        private Vector3 GroundMovement()
        {
            if (!moveWithGround) return Vector3.zero;

            Vector3 platformMovement = Vector3.zero;
            if (Physics.Raycast(controller.transform.position, Vector3.down, out RaycastHit hit, 3, groundLayers) && isGrounded)
            {
                if (Ground != hit.transform)
                {
                    Ground = hit.transform;
                    GroundPrevious = Ground.position;
                }
                GroundCurrent = Ground.position;

                Vector3 groundVelocity = GroundCurrent - GroundPrevious;

                BalancedMovement = Vector3.Slerp(BalancedMovement, groundVelocity, Time.deltaTime * (balance / Time.deltaTime));

                platformMovement = new Vector3(BalancedMovement.x, 0, BalancedMovement.z);

                GroundPrevious = GroundCurrent;

                return platformMovement;
            }
            else Ground = null;
            return platformMovement;
        }

        private IEnumerator Dodge()
        {
            if (dodgeMustBeGrounded && !isGrounded) yield break;

            FreezeMovement = true;

            yield return new WaitForSeconds(dodgeDelay);

            var targetDestination = transform.position + DirectionCalcRaw().normalized * dodgeDistance;

            if (transform.InverseTransformPoint(targetDestination) == Vector3.zero) { targetDestination = transform.position + -transform.forward * dodgeDistance; }

            if (dodgeUsesStanima)
            {
                if (stanima.CurrentValue >= dodgeStanimaCost)
                    stanima.AdjustStatus(-dodgeStanimaCost);
                else
                {
                    FreezeMovement = false;
                    currentDodge = null;
                    yield break;
                }
            }

            var timer = Time.time + 1;
            while (Vector3.Distance(transform.position, targetDestination) > 0.2f && Time.time < timer)
            {
                var movement = dodgeSpeed * Time.deltaTime * (targetDestination - transform.position);
                controller.Move(movement);
                yield return null;
            }

            yield return new WaitForSeconds(dodgeCooldown);

            currentShortJumpMultipler = 1;
            currentFallMultiplier = 1;
            fallForce = 0;
            FreezeMovement = false;
            currentDodge = null;
        }
        #endregion

        #region Input Callbacks
        private void SetupInput()
        {
            if (usePlayerInputComponent)
            {
                if (playerInput) return;
                else SetupPlayerInputComponent();
            }
            input.Enable();
            SetCallback(useCustomInput ? MoveInput : input.Player.Move, OnMove);
            SetCallback(useCustomInput ? SprintInput : input.Player.Sprint, OnSprint);
            SetCallback(useCustomInput ? JumpInput : input.Player.Jump, OnJump);
            SetCallback(useCustomInput ? CrouchInput : input.Player.Crouch, OnCrouch);
            SetCallback(useCustomInput ? DodgeInput : input.Player.Dodge, OnDodge);
        }

        private void SetupPlayerInputComponent()
        {
            if (!playerInput) playerInput = GetComponent<PlayerInput>();
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        public void SetCallback(InputAction action, Action<InputAction.CallbackContext> func)
        {
            if (action == null) return;
            action.started += func;
            action.performed += func;
            action.canceled += func;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            inputDirection = context.ReadValue<Vector2>();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            sprinting = context.performed;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started && currentJumpCount < jumpCount)
            {
                isJumping = true;
                LastJumpHieght = 0;
                currentJumpCount++;
                currentShortJumpMultipler = 1;
                currentFallMultiplier = 1;
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                fallForce = Mathf.Sqrt(jumpHeight * -2f * gravity.y) - gravity.y;
            }
            else if (context.canceled)
            {
                isJumping = false;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started) return;
            isCrouching = context.performed && isGrounded;
            Crouch();
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (currentDodge != null) return;
            if (context.performed) currentDodge = StartCoroutine(Dodge());
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            var GizmosColor = isGrounded ? Color.blue : Color.red;
            Gizmos.color = GizmosColor;
            var position = controller.transform.position;
            var spherePosition = new Vector3(position.x, position.y - ((controller.height / 2) - controller.radius) - groundedOffset, position.z);
            Gizmos.DrawWireSphere(spherePosition, controller.radius + groundedRadius);
        }
    }
}