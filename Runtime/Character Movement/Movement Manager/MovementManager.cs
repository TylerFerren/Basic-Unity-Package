using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class MovementManager : MonoBehaviour
{
    #region Refrences
    [Header("Refrences")]
    [SerializeField] private bool physicisBased;
    [SerializeField, HideIf("physicisBased")] public CharacterController controller = null;
    [SerializeField, ShowIf("physicisBased")] private Rigidbody rigidBody;

    [SerializeField] public Camera cam = null;
    [SerializeField] public Animator anim;
    #endregion

    #region MovementModifers
    private List<IMovementModifier> movementModifiers = new List<IMovementModifier>();

    public void AddMovementModifer(IMovementModifier movementModifier) => movementModifiers.Add(movementModifier);

    public void RemoveMovementModifer(IMovementModifier movementModifier) => movementModifiers.Remove(movementModifier);
    #endregion

    public bool PauseMovement { get; set; } = false;
    public Vector3 Momentum { get; set; }
    public Vector3 LateralMomentum { get; set; }
    private Vector3 PreviousMovement;

    public Vector2 InputDirection { get; private set; }
    public Vector3 RelativeInput { get; private set; }
    public bool IsGrounded { get; private set; }

    [FoldoutGroup("Ground Detection"), SerializeField] private float groundedOffset = -0.05f;
    [FoldoutGroup("Ground Detection"), SerializeField] private float groundedRadius = 0.05f;
    [FoldoutGroup("Ground Detection"), SerializeField] private LayerMask groundLayers = 1 << 0;
    public Collider CurrentGround { get; set; }
    public Vector3 ContactNormal { get; set; } = Vector3.up;

    private float timeTillLand;

    [FoldoutGroup("Events"), SerializeField] private UnityEvent<bool> grounded;
    [FoldoutGroup("Events"), SerializeField] private UnityEvent<float> landTime;

    void Awake()
    {
        anim = GetComponentInParent<Animator>();
        if (!physicisBased && !controller) controller = GetComponentInParent<CharacterController>();
        if (physicisBased && !rigidBody) rigidBody = GetComponentInParent<Rigidbody>();
        if (!cam) cam = Camera.main;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void FixedUpdate()
    {
        RelativeInputCalc();
        Movement();
    }

    private void Movement()
    {
        GroundedCheck();
        Vector3 movement = Vector3.zero;

        if (PauseMovement) {
            movement = Vector3.Lerp(PreviousMovement, Vector3.zero, Time.fixedDeltaTime);
        }
        else {
            foreach (IMovementModifier movementModifier in movementModifiers) {
                movement += movementModifier.MovementVector;
            }
        }
        if (physicisBased)
        {
            rigidBody.AddForce(movement);
        }
        else
        {
            controller.Move(movement);
        }

        PreviousMovement = movement;
    }

    private void LateUpdate()
    {
        Momentum = controller.velocity;
        LateralMomentum = new Vector3(controller.velocity.x, 0, controller.velocity.z);
    }

    //return a non-smoothed direction calculation
    public Vector3 RelativeInputCalc()
    {
        //gets a direction relative to the look vector
        return RelativeInput = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * new Vector3(InputDirection.x, 0, InputDirection.y);
    }

    public Vector3 CameraRelativeInputCalc() {
        return RelativeInput = Quaternion.Euler(cam.transform.eulerAngles.x, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z) * new Vector3(InputDirection.x, 0, InputDirection.y);
    }

    private Collider[] groundCheckCollider;
    //checks if the controller is within a short distance of anything in the ground layers
    public bool GroundedCheck()
    {
        var position = controller.transform.position;
        var spherePosition = new Vector3(position.x, position.y - ((controller.height / 2) - controller.radius) - groundedOffset, position.z);

        if (controller.gameObject.layer == groundLayers)
            groundCheckCollider = Physics.OverlapSphere(spherePosition, controller.radius + groundedRadius);
        else
            groundCheckCollider = Physics.OverlapSphere(spherePosition, controller.radius + groundedRadius, ~controller.gameObject.layer, QueryTriggerInteraction.Ignore);

        foreach (var collider in groundCheckCollider)
        {
            if (!collider.transform.IsChildOf(controller.transform))
            {
                ContactNormalCheck(collider.ClosestPointOnBounds(transform.position));
                CurrentGround = collider;
                timeTillLand = 0;
                if (!IsGrounded) grounded?.Invoke(true);
                return IsGrounded = true;
            }
        }
        ContactNormal = Vector3.up;
        CurrentGround = null;
        TimeTillLand();
        if (IsGrounded) grounded?.Invoke(false);
        return IsGrounded = false;
    }

    public void ContactNormalCheck(Vector3 contactPoint) {
        if (Physics.Raycast(controller.transform.position, contactPoint - transform.position, out RaycastHit hit))
        {
            ContactNormal = hit.normal;
        }
    }

    public void TimeTillLand() {
        if (Physics.Raycast(controller.transform.position, Vector3.down, out RaycastHit hitInfo, 2000)) {
            float groundDistance = hitInfo.distance;
            if (controller.velocity.y < 0)
                timeTillLand = groundDistance / -controller.velocity.y;
            else
                timeTillLand = 100;

            landTime?.Invoke(timeTillLand);
        }
    }

    public void MoveInput(InputAction.CallbackContext context) {
        InputDirection = context.ReadValue<Vector2>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(controller.transform.position, CameraRelativeInputCalc() * 1);

        var GizmosColor = IsGrounded ? Color.blue : Color.red;

        Gizmos.color = GizmosColor;
        var position = controller.transform.position;
        var spherePosition = new Vector3(position.x, position.y - ((controller.height / 2) - controller.radius) - groundedOffset, position.z);
        Gizmos.DrawWireSphere(spherePosition, controller.radius + groundedRadius);

    }
}
