using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

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

        private enum RotationType {MovementBased, PointerBased }

        #region Fields
        [Header("Rotation")]
        [SerializeField] private RotationType rotationType = RotationType.MovementBased;

        [SerializeField, ShowIf("rotationType", RotationType.MovementBased), Tooltip("Always rotates to face camera forward")] private bool lockToCameraForward = false;
        public bool LockToCameraForward { get { return lockToCameraForward; } set { lockToCameraForward = value; } }
        [SerializeField, ShowIf("rotationType", RotationType.MovementBased), Tooltip("Only Rotates when while moving")] private bool onlyRotateOnMove = false;
        public bool OnlyRotateOnMove { get { return onlyRotateOnMove; } set { onlyRotateOnMove = value; } }

        [SerializeField, ShowIf("rotationType", RotationType.PointerBased)] private bool useStandardCursor;

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
        private Vector3 targetDirection;
        #endregion

        private void Awake()
        {
            if (!movementManager) movementManager = GetComponent<MovementManager>();
        }

        private void FixedUpdate()
        {

            if (useStandardCursor) pointerPosition = Input.mousePosition;
            if (!onlyRotateOnMove || movementManager.InputDirection != Vector2.zero || ForceRotate)
                if (rotationType == RotationType.PointerBased) RotateTowardsPointer();
                else RotateByInput();
                //RotationCalc();
        }

        private void RotateTowardsPointer()
        {
            

            // Cast a ray from the camera to the pointer position
            Ray ray = movementManager.cam.ScreenPointToRay(pointerPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetDirection = hit.point - movementManager.controller.transform.position;
                targetDirection.y = 0f; // Ignore vertical component for 3D rotation
            }

            RotatePlayer();
        }

        private void RotateByInput()
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

        //private void RotationCalc()
        //{
        //    // Set rotation speed based on whether the character is grounded
        //    float rotSpeed = movementManager.IsGrounded ? rotationSpeed : airRotationSpeed;
        //    float rotation;

        //    if (rotationType == RotationType.MovementBased)
        //    {
        //        // Set target rotation toward move direction or camera forward
        //        if (lockToCameraForward || movementManager.InputDirection == Vector2.zero)
        //            targetRotation = movementManager.cam.transform.eulerAngles.y;
        //        else
        //            targetRotation = Mathf.Atan2(movementManager.RelativeInput.normalized.x, movementManager.RelativeInput.normalized.z) * Mathf.Rad2Deg;

        //        // Get the angle to rotate the character toward the target rotation
        //        rotation = Mathf.MoveTowardsAngle(movementManager.controller.transform.eulerAngles.y, targetRotation, rotSpeed * Time.fixedDeltaTime);
        //    }
        //    else
        //    {
        //        // Convert the pointer position to world coordinates
        //        Vector3 worldMousePosition = movementManager.cam.ScreenToWorldPoint(new Vector3(pointerPosition.x, pointerPosition.y, movementManager.controller.transform.position.z - movementManager.cam.transform.position.z));

        //        // Calculate the direction from the player to the mouse position
        //        Vector3 direction = worldMousePosition - movementManager.controller.transform.position;

        //        // Calculate the rotation angle based on the direction
        //        //rotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        //        Quaternion targetRotationQ = Quaternion.LookRotation(direction);

        //        rotation = Quaternion.Angle(movementManager.controller.transform.rotation, targetRotationQ);

        //    }

        //    // Rotate to face input direction relative to camera position
        //    movementManager.controller.transform.rotation = Quaternion.Lerp(movementManager.controller.transform.rotation, Quaternion.Euler(0.0f, rotation, 0.0f), rotSpeed * Time.fixedDeltaTime);

        //    // Angle between current heading and input heading
        //    InputAngleChange = Vector3.SignedAngle(movementManager.controller.transform.forward, movementManager.InputDirection, Vector3.up) / 90;
        //}
    }
}
