using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Codesign
{
    public class Gravity : MonoBehaviour, IMovementModifier
    {
        #region MovementModifer
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

        [SerializeField] private Vector3 gravity = Physics.gravity;
        public Vector3 GravityVector { get { return gravity; } }


        void Awake()
        {
            movementManager = GetComponent<MovementManager>();
        }

        private void FixedUpdate()
        {
            if (MovementPaused) return;
            GravityCalac();
        }

        private void GravityCalac()
        {

            MovementVector = new Vector3(0, Mathf.Clamp(movementManager.Momentum.y * Time.fixedDeltaTime, Mathf.NegativeInfinity, 0), 0) + Time.fixedDeltaTime * Time.fixedDeltaTime * gravity;
            MovementVector += Time.fixedDeltaTime * Time.fixedDeltaTime * gravity;

        }
    }

}