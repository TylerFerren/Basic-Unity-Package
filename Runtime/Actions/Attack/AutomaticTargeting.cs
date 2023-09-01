using System.Collections;
using UnityEngine;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

namespace Codesign
{
    public class AutomaticTargeting : MonoBehaviour
    {
        public enum TargetSortMethod { closest, furthest, weakest, strongest }

        [field: SerializeField] public float DetectionRange { get; set; }
        [field: SerializeField] public LayerMask DetectionLayers { get; set; }
        [field: SerializeField] public List<GameObject> ObjectsToIgnore { get; set; }

        [SerializeField] private TargetSortMethod sortMethod;
        [field: SerializeField, Range(0, 1)] public float DistanceWeight { get; set; }
        [field: SerializeField, Range(0, 1)] public float ForwardWeight { get; set; }

        private Collider targetedObject;
        public Collider TargetedObject { get { return targetedObject; } }
        public List<TargetObject> ObjectsTargeted = new List<TargetObject>();

        private void Update()
        {
            Targeting();
        }

        protected void Targeting()
        {
            //Searches for a list of targets every fixed update
            var targetOptions = Physics.OverlapSphere(transform.position, DetectionRange, DetectionLayers).ToList();

            targetOptions = RemoveIgnoredObjects(targetOptions);

            List<TargetObject> targetObjects = new List<TargetObject>();
            
            foreach (Collider collider in targetOptions)
            {
                var distance = Vector3.Distance(transform.position, collider.transform.position) / DetectionRange;

                var angelOffset = Vector3.Angle(transform.forward, collider.transform.position - transform.position) / 180;

                distance = (1 - Mathf.Clamp01(distance)) * 0.5f;
                angelOffset = (1 - Mathf.Clamp01(angelOffset)) * 0.5f;
                
                float weight = (angelOffset * ForwardWeight) + (distance * DistanceWeight);
                targetObjects.Add(new TargetObject(collider, weight));    
            }

            targetObjects.Sort((a, b) => b.targetWeight.CompareTo(a.targetWeight));

            ObjectsTargeted = targetObjects;
            
            /*targetOptions.Sort(
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
            );*/

            if (targetOptions.FirstOrDefault())
            {
                targetedObject = targetObjects.FirstOrDefault().collider;
            }
        }

        public List<Collider> RemoveIgnoredObjects(List<Collider> targets)
        {   
            foreach(GameObject gameObject in ObjectsToIgnore)
            {
                targets.RemoveAll(t => t.gameObject.transform.IsChildOf(gameObject.transform));
            }
            return targets;
        }

        public void OnDrawGizmosSelected()
        {
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, Vector3.up, DetectionRange);
            Gizmos.color = Color.yellow;
            if(targetedObject) Gizmos.DrawSphere(targetedObject.bounds.center + new Vector3(0, targetedObject.bounds.extents.y + 0.5f, 0), 0.1f);
            Gizmos.color= Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 3);
        }
    }

    [Serializable]
    public struct TargetObject {
        public Collider collider;
        public float targetWeight;

        public TargetObject(Collider _collider, float _targetWeight) {
            collider = _collider;
            targetWeight = _targetWeight;
        }
    }
}