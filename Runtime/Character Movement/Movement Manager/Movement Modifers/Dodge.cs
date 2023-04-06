using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Codesign
{
    public class Dodge : MonoBehaviour, IMovementModifier
    {
        #region MovementModifier
        public Vector3 MovementVector { get; private set; }
        public bool MovementPaused { get; private set; }

        private void OnEnable() => movementManager.AddMovementModifer(this);
        private void OnDisable() => movementManager.RemoveMovementModifer(this);

        public void PauseMovement(bool paused)
        {
            MovementPaused = paused;
        }
        #endregion

        [SerializeField] private LevelingValue<float> dodgeSpeed = 4;
        public void UpdateDodgeSpeed() => dodgeSpeed.LevelUp();

        [SerializeField] private LevelingValue<float> dodgeDistance = 5;
        public void UpdateDodgeDistance() => dodgeDistance.LevelUp();

        [SerializeField] private float dodgeDelay = 1;
        [SerializeField] private float dodgeCooldown = 1;
        [SerializeField] private bool mustBeGrounded;
        [SerializeField] private InputActionReference dodgeInput;

        private CharacterController controller = null;
        private MovementManager movementManager = null;
        private Camera cam;
        private Coroutine currentDodge;

        private float timer;
        private Vector3 smoothVector;

        [SerializeField, FoldoutGroup("Events")] private UnityEvent dodged;


        void Awake()
        {
            movementManager = GetComponent<MovementManager>();
            controller = movementManager.controller;
            cam = movementManager.cam;
            if (dodgeInput) dodgeInput.action.performed += OnDodge;
        }


        public void OnDodge(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                if (currentDodge != null || MovementPaused) return;
                currentDodge = StartCoroutine(MoveToPosition());
            }
        }

        private Vector3 DodgeVectorCalc()
        {
            Vector3 forwardVector = Vector3.Cross(cam.transform.right, Vector3.up);

            Vector3 destination = movementManager.RelativeInput != Vector3.zero ?
                movementManager.RelativeInput * dodgeDistance :
                controller.transform.forward * dodgeDistance;

            return destination;
        }

        private Vector3 dodgeVector;
        public IEnumerator MoveToPosition()
        {
            if (mustBeGrounded && !movementManager.IsGrounded) yield break;

            movementManager.PauseMovement = true;

            dodgeVector = DodgeVectorCalc();

            var i = 0f;
            while (i < dodgeDelay)
            {
                i += Time.fixedDeltaTime;
                if (movementManager.RelativeInput != Vector3.zero) dodgeVector = DodgeVectorCalc();
                yield return new WaitForFixedUpdate();
            }
            movementManager.PauseMovement = false;

            timer = 0;

            var dodgeTime = dodgeDistance / dodgeSpeed;

            dodged?.Invoke();

            while (timer < 1)
            {
                timer += Time.fixedDeltaTime / dodgeTime;

                smoothVector = Vector3.Lerp(Vector3.zero, Vector3.Lerp(Vector3.zero, dodgeVector * 2, Mathf.Sin(Mathf.PI * timer)), Mathf.Sin(Mathf.PI * timer));

                MovementVector = Time.fixedDeltaTime / dodgeTime * smoothVector;
                yield return new WaitForFixedUpdate();
            }

            MovementVector = Vector3.zero;

            yield return new WaitForSeconds(dodgeCooldown);

            currentDodge = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(transform.position, dodgeVector);
        }

    }
}