using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace Codesign
{
    public class WallRunning : MonoBehaviour, IMovementModifier
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

        [SerializeField] private LevelingValue<float> wallMovementSpeed = 6;
        public void UpdateWallMovementSpeed() => wallMovementSpeed.LevelUp();

        [SerializeField] private float wallStickDistance;
        [SerializeField] private float wallStickSpeed;

        [SerializeField, Range(0, 180)] private float wallReleaseInputAngle;

        [SerializeField, FoldoutGroup("Events")] private UnityEvent OnWallStick;
        [SerializeField, FoldoutGroup("Events")] private UnityEvent OnWallRelease;

        private bool stickingToWall;
        private Vector3 gravity = Physics.gravity;
        private Collider wallCollider;
        private Vector3 wallNormal;

        private Vector3 projectedCalc;
        private Vector3 projectedInput;

        private void Awake()
        {
            if (!movementManager) movementManager = GetComponent<MovementManager>();
            gravity = movementManager.Gravity;
        }

        private void FixedUpdate()
        {
            if (MovementPaused) return;

            Vector3 moveCalc;

            if (StickingToWall()) {
                if (!stickingToWall) {
                    if(movementManager.IsGrounded) movementManager.ContactNormal = wallNormal;
                    OnWallStick?.Invoke();
                    stickingToWall = true;
                }
                moveCalc = WallRunCalc();
                if (!movementManager.IsGrounded) movementManager.ContactNormalCheck(wallCollider.ClosestPoint(movementManager.transform.position));
            }
            else {
                if (stickingToWall) {
                    OnWallRelease?.Invoke();
                    stickingToWall = false;
                }
                moveCalc = Vector3.zero;
            }
            var gravitationalForce = Vector3.Lerp(Vector3.zero, Time.fixedDeltaTime * -gravity, movementManager.LateralMomentum.magnitude / wallStickSpeed);
        
            MovementVector = Vector3.Lerp(MovementVector, wallMovementSpeed * Time.fixedDeltaTime * moveCalc, Time.fixedDeltaTime * 3);
            if (MovementVector.magnitude / Time.fixedDeltaTime < 0.01f) MovementVector = Vector3.zero;
        }

        private Vector3 WallRunCalc()
        {
            Vector3 inputDir = movementManager.RelativeInputCalc();

            if (Vector3.Angle(inputDir, wallNormal) > 90)
            {
                Quaternion rot = quaternion.AxisAngle(Vector3.Cross(movementManager.controller.transform.up, wallNormal), Vector3.Angle(movementManager.controller.transform.up, wallNormal));
                projectedCalc = Vector3.ProjectOnPlane(rot * inputDir, wallNormal);
            }
            else if (Vector3.Angle(inputDir, wallNormal) > wallReleaseInputAngle) {
                projectedCalc = Vector3.ProjectOnPlane(inputDir, wallNormal);
            }
            else projectedCalc = inputDir;

            projectedCalc = Vector3.Lerp(projectedCalc, movementManager.Momentum, Time.fixedDeltaTime);

            return projectedInput = Vector3.Lerp(projectedInput, projectedCalc, Time.fixedDeltaTime * 3);
        }

        private bool StickingToWall() {
            var hits = Physics.OverlapSphere(movementManager.controller.transform.position + (Vector3.up * wallStickDistance), wallStickDistance, 0, QueryTriggerInteraction.Ignore).ToList();
            hits.RemoveAll(c => c.transform.root == movementManager.transform.root);
       
            if (hits.Count <= 0) {
                wallCollider = null;
                return false;
            }

            wallCollider = hits.FirstOrDefault();
            var lineCast = Physics.Linecast(movementManager.controller.transform.position + Vector3.up, wallCollider.ClosestPointOnBounds(movementManager.controller.transform.position + Vector3.up), out RaycastHit hit);

            wallNormal = hit.normal;

            if (Vector3.Angle(movementManager.RelativeInputCalc(), wallNormal) < wallReleaseInputAngle) return false; 
            else return true;
        
        }

        private void OnDrawGizmosSelected()
        {
            if (stickingToWall) Gizmos.color = Color.yellow;
            else Gizmos.color = Color.white;
            if (movementManager) {
                Gizmos.DrawWireSphere(movementManager.controller.transform.position + (Vector3.up * wallStickDistance), wallStickDistance);
            }

            if (wallCollider != null) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(movementManager.controller.transform.position, movementManager.controller.transform.position + projectedInput);
                Gizmos.DrawSphere(movementManager.controller.transform.position + projectedInput, 0.1f);
            }

        }

    }
}