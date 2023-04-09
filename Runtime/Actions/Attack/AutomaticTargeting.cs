using System.Collections;
using UnityEngine;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.OdinInspector;

namespace Codesign
{
    public class AutomaticTargeting : MonoBehaviour
    {
        public enum TargetSortMethod {closest, furthest, weakest, strongest }

        [SerializeField] private float targetDetectionRange;
        [SerializeField] private LayerMask detectionLayers;

        [SerializeField] private TargetSortMethod sortMethod;

        private Collider targetedObject;
        public Collider TargetedObject { get { return targetedObject; } }

        private void Update()
        {
            Targeting();
        }

        protected void Targeting()
        {
            //Searches for a list of targets every fixed update
            var targetOptions = Physics.OverlapSphere(transform.position, targetDetectionRange, detectionLayers);

            targetOptions.Sort(
                (a, b) =>
                {
                    float distA = Vector3.Distance(transform.position, a.ClosestPointOnBounds(transform.position));
                    float distB = Vector3.Distance(transform.position, b.ClosestPointOnBounds(transform.position));
                    Health healthA = a.GetComponentInChildren<Health>();
                    Health healthB = b.GetComponentInChildren<Health>();
                    return sortMethod switch
                    {
                        TargetSortMethod.closest => distA.CompareTo(distB),
                        TargetSortMethod.furthest => distB.CompareTo(distA),
                        TargetSortMethod.weakest => healthA.CurrentValue.CompareTo(healthB.CurrentValue),
                        TargetSortMethod.strongest => healthB.CurrentValue.CompareTo(healthA.CurrentValue),
                        _ => 1,
                    };
                }
            );

            if (targetOptions.FirstOrDefault())
            {
                targetedObject = targetOptions.FirstOrDefault();
            }
        }

        public void OnDrawGizmosSelected()
        {
            Handles.DrawWireDisc(transform.position, Vector3.up, targetDetectionRange);
            if(targetedObject) Gizmos.DrawSphere(targetedObject.bounds.center + new Vector3(0, targetedObject.bounds.extents.y + 0.5f, 0), 0.5f);
        }
    }
}