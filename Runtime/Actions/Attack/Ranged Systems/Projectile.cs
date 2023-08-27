using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    public enum FlightPath { straight, Arc}
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private LayerMask detectibleLayers;
        [SerializeField] private float projectileSpeed =10;
        [field: SerializeField] public FlightPath Path;
        [SerializeField, ShowIf("Path", FlightPath.Arc)] private float arcHeight;
        [field: SerializeField] public float AreaOfEffect { get; set; }

        private Vector3 origin;
        private float FlightTime;
        private float startTime;

        public IEnumerator Fire(Vector3 Destination, RangedAttack action)
        {
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

            if (Path == FlightPath.Arc) Destination.y = 0;

            FlightTime = Vector3.Distance(Destination, origin) / projectileSpeed;

            Vector3 previousLocation = transform.position;
            detectibleLayers = action.AttackableLayers;
            while (Vector3.Distance(transform.position, Destination) > projectileSpeed * Time.deltaTime && gameObject.activeInHierarchy)
            {
                transform.position = CalcFlightPath(Destination);

                var ColliderHit = Physics.Linecast(previousLocation, transform.position, out RaycastHit hit, detectibleLayers, QueryTriggerInteraction.Ignore);

                if (ColliderHit && !hit.collider.transform.IsChildOf(actor.transform))
                {
                    action.Hit(hit.collider);
                    OnHit(action);
                    
                    if (AreaOfEffect > 0)
                    {
                        var CollidersHit = Physics.OverlapSphere(transform.position, AreaOfEffect, detectibleLayers, QueryTriggerInteraction.Ignore);
                        CollidersHit = CollidersHit.Where(s => s != hit.collider).ToArray();
                        foreach (Collider collider in CollidersHit)
                        {
                            var distance = Vector3.Distance(transform.position, collider.transform.position) / AreaOfEffect;
                            action.Hit(collider, distance);
                            OnHit(action);
                        }
                    yield break;
                    }
                }
                yield return new WaitForFixedUpdate();
            }
            OnHit(action);
            yield break;
        }

        private void OnHit(RangedAttack action) {
            StopAllCoroutines();
            if (action.poolProjectile)
            {
                transform.parent = action.Pooler.transform;
                gameObject.SetActive(false);
            }
            else
                Destroy(gameObject);
        }

        private Vector3 CalcFlightPath(Vector3 Destination) {
            switch (Path) {
                case FlightPath.straight:
                        return Vector3.MoveTowards(transform.position, Destination, projectileSpeed * Time.deltaTime);
                case FlightPath.Arc:
                    Vector3 CenterPoint1 = Vector3.Lerp(origin, Destination, 0.33f) + (Vector3.up * arcHeight);
                    Vector3 CenterPoint2 = Vector3.Lerp(origin, Destination, 0.67f) + (Vector3.up * arcHeight);
                    Vector3[] points = new Vector3[4] { origin, CenterPoint1, CenterPoint2, Destination};

                    FlightTime = (Vector3.Distance(origin, CenterPoint1) + Vector3.Distance(CenterPoint1, CenterPoint2) + Vector3.Distance(CenterPoint2, Destination)) / projectileSpeed;

                    transform.LookAt(CalculateBezierPoint(((Time.time + (FlightTime/10)) - startTime) / FlightTime, points));

                    return CalculateBezierPoint((Time.time - startTime) / FlightTime, points);
            }
            return Vector3.zero;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3[] points)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * points[0];
            p += 3 * uu * t * points[1];
            p += 3 * u * tt * points[2];
            p += ttt * points[3];

            return p;
        }
    }
}
