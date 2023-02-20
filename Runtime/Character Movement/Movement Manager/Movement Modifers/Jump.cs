using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Codesign
{
    [RequireComponent(typeof(MovementManager))]
    public class Jump : MonoBehaviour, IMovementModifier
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

        [SerializeField] private LevelingValue<float> jumpHeight = new LevelingValue<float>(3f);
        public void UpdateJumpHeight() => jumpHeight.LevelUp();

        [SerializeField] private LevelingValue<int> jumpCount = new LevelingValue<int>(1);
        public void UpdateJumpCount() => jumpCount.LevelUp();


        [SerializeField, Range(1f, 10f)] private float shortJumpMultipler = 2;

        [SerializeField, Range(0, 1f)] private float contactNormalInfluence;
        [SerializeField, Range(0, 1f)] private float inputInfluence;

        [SerializeField] private InputActionReference jumpInput;

        public float LastJumpHieght { get; set; } = 0;

        private bool isJumping;
        private int currentJumpCount;
        private float jumpForce;
        private Vector3 jumpVector;
        private Vector3 gravity = Physics.gravity;

        private float currentShortJumpMultipler;

        private float jumpingFloor;

        [SerializeField, FoldoutGroup("Events")] private UnityEvent jumped;

        void Awake()
        {
            movementManager = GetComponent<MovementManager>();

            if (TryGetComponent(out Gravity _gravity))
            {
                gravity = _gravity.GravityVector;
            }

            if (jumpInput)
            {
                jumpInput.action.performed += OnJump;
                jumpInput.action.canceled += OnJump;
            }
        }

        private void FixedUpdate()
        {
            if (MovementPaused) return;

            if (movementManager.IsGrounded && !isJumping)
            {
                jumpVector = Vector3.Lerp(jumpVector, Vector3.zero, Time.deltaTime * 5);
            }

            MovementVector = JumpCalc();


            if (!movementManager.IsGrounded && movementManager.controller.transform.position.y - jumpingFloor > LastJumpHieght)
                LastJumpHieght = movementManager.controller.transform.position.y - jumpingFloor;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started && currentJumpCount < jumpCount.Value)
            {
                if (movementManager.IsGrounded)
                {
                    jumpingFloor = movementManager.controller.transform.position.y;
                    LastJumpHieght = 0;
                }
                isJumping = true;
                jumped?.Invoke();
                currentJumpCount++;
                currentShortJumpMultipler = 1;

                // the square root of H * -2 * G = how much velocity needed to reach desired height
                jumpForce = Mathf.Sqrt(jumpHeight.Value * -2f * gravity.y);
                jumpVector = Vector3.Lerp(Vector3.Lerp(Vector3.up, movementManager.ContactNormal, contactNormalInfluence), movementManager.RelativeInput, inputInfluence).normalized * jumpForce;
            }
            else if (context.canceled)
            {
                isJumping = false;
            }
        }

        private Vector3 JumpCalc()
        {
            if (movementManager.IsGrounded && jumpVector.y <= 0 && currentJumpCount != 0)
            {
                ResetJump();
            }
            //checks if the charcter is grounded but not trying to jump
            if (jumpVector.y > 0)
            {
                if (!isJumping)
                {
                    currentShortJumpMultipler = Mathf.Lerp(currentShortJumpMultipler, shortJumpMultipler, Time.fixedDeltaTime * 5);
                }
                jumpVector += gravity * currentShortJumpMultipler * Time.deltaTime;
            }
            else if (MovementVector == Vector3.zero)
            {
                jumpVector = Vector3.zero;
            }

            return Time.fixedDeltaTime * jumpVector;
        }

        public void ResetJump()
        {
            currentJumpCount = 0;
        }
    }
}