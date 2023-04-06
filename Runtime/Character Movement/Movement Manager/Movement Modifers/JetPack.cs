using System.Collections;
using System.Collections.Generic;
using Codesign;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codesign {
    public class JetPack : MonoBehaviour, IMovementModifier
    {
        #region MovementModifier
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

        [SerializeField] private Vector3 BoostVector;
        [SerializeField] private float BoostAccelleration = 1;
        [SerializeField] private float BoostDeceleration = 1;

        [SerializeField] private bool UseStatus;
        [SerializeField, ShowIf("UseStatus")] private Status status;
        [SerializeField, ShowIf("UseStatus")] private float drainRate;

        private bool boosting;

        public void Awake()
        {
            if (!movementManager) movementManager = GetComponent<MovementManager>();
        }

        public void OnBoost(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                boosting = true;
            }

            if (context.canceled)
            {
                boosting = false;
            }
        }

        public void FixedUpdate() {
            MovementVector = Boost();
        }

        public bool StatusCheck() {
            if (!UseStatus) return true;
            else {
                if (status == null) {
                    Debug.LogWarning(gameObject.name + ": Jetpack does not have a status assigned");
                    return true;
                }
                if (status.CurrentValue > 0)
                {
                    return true;
                }
                return false;
            }
        }

        private Vector3 Boost() {
            if (boosting && StatusCheck())
            {
                if (UseStatus && status != null) status.AdjustStatus(-drainRate * Time.fixedDeltaTime);

                var boostForce = new Vector3(0, BoostVector.y, 0) + movementManager.cam.transform.TransformVector(new Vector3(BoostVector.x, 0, BoostVector.z));
                boostForce -= movementManager.Gravity * Time.fixedDeltaTime;
                boostForce *= Time.fixedDeltaTime;
                return Vector3.Lerp(MovementVector, boostForce, Time.fixedDeltaTime * BoostAccelleration);

            }
            else if (MovementVector.magnitude > 0.01)
            {
                return Vector3.Lerp(MovementVector, Vector3.zero, Time.fixedDeltaTime * BoostAccelleration * BoostDeceleration);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}
