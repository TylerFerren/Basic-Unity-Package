using System.Collections;
using UnityEngine;
using System.Linq;
using Sirenix.Utilities;

namespace Codesign
{
    public class AutomaticTargeting : MonoBehaviour
    {
        [SerializeField] private float targetDetectionRange;
        [SerializeField] private LayerMask detectionLayers;
        [SerializeField] private Collider targetedObject;
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
                    float distA = Vector3.Distance(transform.position, a.transform.position);
                    float distB = Vector3.Distance(transform.position, b.transform.position);
                    return distA.CompareTo(distB);
                }
            );

            if (targetOptions.FirstOrDefault())
            {
                targetedObject = targetOptions.FirstOrDefault();
            }
        }
    }
}