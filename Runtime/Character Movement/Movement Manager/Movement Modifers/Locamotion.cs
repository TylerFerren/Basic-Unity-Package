using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Codesign
{
    [RequireComponent(typeof(MovementManager))]
    public class Locamotion : MonoBehaviour, IMovementModifier
    {
        #region IMovementModifer
        public Vector3 MovementVector { get; private set; }
        public bool MovementPaused { get; private set; }

        private void OnEnable() => movementManager.AddMovementModifer(this);
        private void OnDisable() => movementManager.RemoveMovementModifer(this);

        public void PauseMovement(bool paused)
        {
            MovementPaused = paused;
            if (MovementPaused) MovementVector = Vector3.zero;
        }
        #endregion

        private MovementManager movementManager = null;


        #region fields
        [SerializeField, Tooltip("Base Speed of standard movement")] private LevelingValue<float> standardSpeed = 5;
        public float StandardSpeed { get { return standardSpeed; } set { standardSpeed = value; } }
        [SerializeField] private float acceleration = 5;
        public float Acceleration { get { return acceleration; } set { acceleration = value; } }
        [SerializeField] private float directionalAcceleration = 3;
        public float DirectionalAcceleration { get { return directionalAcceleration; } set { directionalAcceleration = value; } }

        [SerializeField] private AnimationCurve offAngleSpeedReduction = AnimationCurve.Constant(0, 180, 1);
        [SerializeField] private AnimationCurve slopeSpeedReduction = AnimationCurve.Constant(0, 180, 1);

        [Header("Sprint")]
        [SerializeField, Tooltip("Top Speed of standard movement")] private LevelingValue<float> sprintSpeed = 9;
        [SerializeField] private bool sprintUsesStatus = false;
        [SerializeField, ShowIf("sprintUsesStatus")] private Status status;
        [SerializeField, ShowIf("sprintUsesStatus")] private LevelingValue<float> sprintStatusCost = 3;



        [Header("Air")]
        [SerializeField, Tooltip("movement speed when not grounded")] private LevelingValue<float> airMoveSpeed = 3f;
        [SerializeField, Tooltip("adjusts amount of momentum lost when airborn")] private float airResitance = 1f;
        #endregion

        #region modifiers
        private bool sprinting;
        private float speedModifier = 1;
        public float SpeedModifier { get { return speedModifier; } set {speedModifier = value;} }
        #endregion

        #region outputs
        public Vector3 Direction { get; private set; }
        public Vector3 TargetDirection { get; private set; }
        public float InputAngleChange { get; private set; }
        #endregion

        #region local
        private float currentSpeed;
        private float targetSpeed;
        #endregion

        #region events
        [SerializeField, FoldoutGroup("Events")] private UnityEvent<bool> IsSprinting;
        [SerializeField, FoldoutGroup("Events")] private UnityEvent<Vector3> IsMoving;
        [SerializeField, FoldoutGroup("Events")] private UnityEvent<Vector3> IsMovingScaled;
        #endregion

        private void Awake()
        {
            if (!movementManager) movementManager = GetComponent<MovementManager>();
        }

        private void FixedUpdate()
        {
            if (MovementPaused) return;

            MovementVector = Time.fixedDeltaTime * SpeedCalc() * DirectionCalc();

            IsMoving?.Invoke(MovementVector / Time.fixedDeltaTime);
            IsMovingScaled?.Invoke(MovementVector / sprintSpeed / Time.fixedDeltaTime);

            //Starts using Energy if target speed is close to sprint speed
            if (sprintUsesStatus && status != null && sprintSpeed - targetSpeed <= 1)
            {
                status.AdjustStatus(-sprintStatusCost * Time.fixedDeltaTime);
            }
        }

        private Vector3 DirectionCalc()
        {
            var relativeInput = movementManager.RelativeInput;

            if (movementManager.IsGrounded)
            {
                var groundRelativeDir = Vector3.ProjectOnPlane(relativeInput, movementManager.ContactNormal).normalized;
                TargetDirection = Vector3.Distance(TargetDirection, groundRelativeDir) > 0.01f ? Vector3.Lerp(TargetDirection, groundRelativeDir, Time.fixedDeltaTime * directionalAcceleration) : groundRelativeDir;
            }
            else
                TargetDirection = Vector3.Distance(TargetDirection, relativeInput) > 0.01f ? Vector3.Lerp(TargetDirection, relativeInput, Time.fixedDeltaTime * directionalAcceleration) : relativeInput;

            //smoothes out direction
            Direction = Vector3.Distance(Direction, TargetDirection) > 0.01f ? Vector3.Lerp(Direction, TargetDirection, Time.fixedDeltaTime * directionalAcceleration) : TargetDirection;

            if (!movementManager.IsGrounded) Direction = new Vector3(Direction.x, 0, Direction.z);

            return Direction;
        }

        private float SpeedCalc()
        {
            //Checks if the character is grounded and/or sprinting and sets speed accordingly
            if (movementManager.InputDirection == Vector2.zero) targetSpeed = 0;
            else if (movementManager.IsGrounded)
            {
                //Checks if sprinting
                if (sprinting) targetSpeed = sprintSpeed;
                else targetSpeed = standardSpeed;
            }
            else
            {
                //keeps momentum at the begining of a jump
                targetSpeed = currentSpeed > airMoveSpeed ? Mathf.Lerp(currentSpeed, airMoveSpeed, Time.fixedDeltaTime * airResitance) : airMoveSpeed;
            }

            //adjust speed by offAngleSpeedReduction - input direction vs facing direction
            var relativeFacingAngle = Vector3.SignedAngle(transform.forward, Direction, Vector3.up);
            targetSpeed *= offAngleSpeedReduction.Evaluate(relativeFacingAngle);

            //Debug.Log(Vector3.SignedAngle(movementManager.ContactNormal, Vector3.up, Vector3.right));
            targetSpeed *= slopeSpeedReduction.Evaluate(Vector3.SignedAngle(movementManager.ContactNormal, Vector3.up, Vector3.right));


            targetSpeed *= SpeedModifier;

            //smooths speed towards target speed
            if (currentSpeed < targetSpeed - 0.1f || currentSpeed > targetSpeed + 0.1f)
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);
            else
                currentSpeed = targetSpeed;

            //rounds to hundths place
            currentSpeed = Mathf.Round(currentSpeed * 100f) / 100f;

            return currentSpeed;
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            if (sprintUsesStatus && status.CurrentValue <= 0) 
                sprinting = false;
            else 
                sprinting = context.performed;

            IsSprinting?.Invoke(sprinting);
        }
    }
}