using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoosterPack : MonoBehaviour, IMovementModifier
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

    [SerializeField] private float BoostLift;
    [SerializeField, Range(0.5f, 10)] private float BoostDuration = 1;
    [SerializeField] private float BoostDistance;

    [SerializeField] private bool MustNotBeGrounded;

    private Vector3 boostVector;
    private Vector3 smoothBoost;

    private bool boosting;

    private Vector3 boostDestination;

    public void OnBoost(InputAction.CallbackContext context) {
        if (MustNotBeGrounded && movementManager.IsGrounded) return;
        boosting = context.performed;

        if (context.started) {
            boostDestination = movementManager.controller.transform.position + movementManager.controller.transform.forward * BoostDistance;
        }
    }

    private void Awake()
    {
        if (!movementManager) movementManager = GetComponent<MovementManager>();

    }

    private void FixedUpdate()
    {
        if (MovementPaused) return;

        if (boosting || boostVector.magnitude > 0) BoostCalc();

        MovementVector = boostVector;
    }

    private Vector3 BoostCalc() {
        Vector3 boostDirection = boostDestination - movementManager.controller.transform.position;

        if (boostDirection.magnitude < 0.5f) return boostVector = Vector3.zero;

        Vector3 boostHeight = Vector3.up * BoostLift;

        smoothBoost = Vector3.Lerp(smoothBoost, boostDirection, Time.fixedDeltaTime);
        
        return boostVector = boostDirection * Time.fixedDeltaTime / BoostDuration;
    }

    private void OnDrawGizmosSelected()
    {
        if (boostDestination != null)
        {
            Gizmos.DrawCube(boostDestination, Vector3.one * 0.5f);

        } 
    }

}
