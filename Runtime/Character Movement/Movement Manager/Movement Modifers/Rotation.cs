using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codesign
{
    [RequireComponent(typeof(MovementManager))]
    public class Rotation : MonoBehaviour, IMovementModifier
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

        private enum RotationType {MovementBased, InputBased }

        #region Fields
        [Header("Rotation")]
        [SerializeField] private RotationType rotationType = RotationType.MovementBased;

        [SerializeField, ShowIf("rotationType", RotationType.MovementBased), Tooltip("Always rotates to face camera forward")] private bool lockToCameraForward = false;
        public bool LockToCameraForward { get { return lockToCameraForward; } set { lockToCameraForward = value; } }
        [SerializeField, ShowIf("rotationType", RotationType.MovementBased), Tooltip("Only Rotates when while moving")] private bool onlyRotateOnMove = false;
        public bool OnlyRotateOnMove { get { return onlyRotateOnMove; } set { onlyRotateOnMove = value; } }

        [SerializeField, ShowIf("rotationType", RotationType.InputBased)] private bool useStandardCursor;

        [SerializeField, Range(0.0f, 720f),] private float rotationSpeed = 180f;
        public bool ForceRotate { get; set; } = false;

        [Header("Air")]
        [SerializeField, Range(0.0f, 720f),] private float airRotationSpeed = 150f;
        #endregion

        #region Outputs
        public float InputAngleChange { get; private set; }
        #endregion

        #region Local
        private float targetRotation;
        private Vector3 pointerPosition;
        private Vector2 inputDirection;
        private Vector3 targetDirection;
        #endregion

        public void OnLook(InputAction.CallbackContext context) { 
            inputDirection = context.ReadValue<Vector2>();
        }

        private void Awake()
        {
            if (!movementManager) movementManager = GetComponent<MovementManager>();
        }

        private void FixedUpdate()
        {

            
            if (!onlyRotateOnMove || movementManager.InputDirection != Vector2.zero || ForceRotate)
                if (rotationType == RotationType.InputBased) RotateTowardsInput();
                else RotateByMovement();
        }

        private void RotateTowardsInput()
        {
            /*
            if (useStandardCursor)
            {
                pointerPosition = Input.mousePosition;
                // Cast a ray from the camera to the pointer position
                Ray ray = movementManager.cam.ScreenPointToRay(pointerPosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    targetDirection = hit.point - movementManager.controller.transform.position;
                    targetDirection.y = 0f; // Ignore vertical component for 3D rotation
                }
            }
            */
            
            if(inputDirection != Vector2.zero)
                targetDirection = Vector3.Lerp(targetDirection, new Vector3(inputDirection.x, 0, inputDirection.y), Time.fixedDeltaTime * 3);
            RotatePlayer();
        }

        private void RotateByMovement()
        {
            //Set target rotation toward move direction or camera forward
            if (lockToCameraForward || movementManager.InputDirection == Vector2.zero)
                targetDirection = Vector3.zero;
            else
                targetDirection = movementManager.RelativeInput.normalized;
            
            RotatePlayer();
        }

        private void RotatePlayer()
        {
            if (targetDirection != Vector3.zero)
            {
                float rotSpeed = movementManager.IsGrounded ? rotationSpeed : airRotationSpeed;
                //rotSpeed /= 90;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                //movementManager.controller.transform.rotation = Quaternion.Lerp(movementManager.controller.transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime);

                movementManager.controller.transform.rotation = Quaternion.RotateTowards(movementManager.controller.transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime);
            }
        }

    }
}
