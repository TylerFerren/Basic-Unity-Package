using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Codesign {
    public enum FlightPath { straight, Arc}
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private LayerMask detectibleLayers;
        [SerializeField] private float projectileSpeed =10;
        [field: SerializeField] public FlightPath Path;
        [SerializeField, ShowIf("Path", FlightPath.Arc)] private float arcHeight;

        private Vector3 origin;
        private float FlightTime;
        private float startTime;

        public IEnumerator Fire(Vector3 Destination, RangedAttack action)
        {
            print("projectile Trigger");
            origin = action.transform.TransformPoint(action.FirePoint);
            startTime = Time.time;
            FlightTime = Vector3.Distance(Destination, origin) / projectileSpeed;
            Vector3 previousLocation = transform.position;
            detectibleLayers = action.AttackableLayers;
            while (transform.position != Destination) {
                CalcFlightPath(Destination);

                var ColliderHit = Physics.Linecast(previousLocation, transform.position, out RaycastHit hit, detectibleLayers, QueryTriggerInteraction.Ignore);
                
                if (ColliderHit) {
                    action.Hit(hit.collider);
                    OnHit(action);
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
            OnHit(action);
            yield break;
        }

        public IEnumerator Fire(Vector3 Destination, RangedAttack action, GameObject actor)
        {
            origin = action.transform.TransformPoint(action.FirePoint);
            startTime = Time.time;
            FlightTime = Vector3.Distance(Destination, origin) / projectileSpeed;
            Vector3 previousLocation = transform.position;
            detectibleLayers = action.AttackableLayers;
            while (transform.position != Destination)
            {
                CalcFlightPath(Destination);

                var ColliderHit = Physics.Linecast(previousLocation, transform.position, out RaycastHit hit, detectibleLayers, QueryTriggerInteraction.Ignore);

                if (ColliderHit && !hit.collider.transform.IsChildOf(actor.transform))
                {
                    action.Hit(hit.collider);
                    OnHit(action);
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
            print(Destination + "    :    " + origin);
            OnHit(action);
            yield break;
        }

        private void OnHit(RangedAttack action) {
            print("On Hit");
            if (action.poolProjectile)
            {
                transform.parent = action.Pooler.transform;
                gameObject.SetActive(false);
            }
            else
                Destroy(gameObject);
        }

        private void CalcFlightPath(Vector3 Destination) {
            print("Calc path");
            switch (Path) {
                case FlightPath.straight:
                        transform.position = Vector3.MoveTowards(transform.position, Destination, projectileSpeed * Time.deltaTime);
                    break;
                case FlightPath.Arc:
                    //needs an arc path
                    transform.position = Vector3.Slerp(origin, Destination, (Time.time - startTime) / FlightTime);


                    break;
            }
        }
    }
}
