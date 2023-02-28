using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Codesign {
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private LayerMask detectibleLayers;
        [SerializeField] private float projectileSpeed =10;

        public IEnumerator Fire(Vector3 Destination, RangedAttack action)
        {
            Vector3 previousLocation = transform.position;
            detectibleLayers = action.AttackableLayers;
            while (transform.position != Destination) {
                transform.position = Vector3.MoveTowards(transform.position, Destination, projectileSpeed * Time.deltaTime);

                var ColliderHit = Physics.Linecast(previousLocation, transform.position, out RaycastHit hit, detectibleLayers, QueryTriggerInteraction.Ignore);
                
                if (ColliderHit && !action.ActorColliers.Contains(hit.collider)) {
                    hit.collider.TryGetComponent(out Health health);
                    action.Hit(hit, health);
                    OnHit(action);
                    yield break;
                }
                yield return new WaitForFixedUpdate();
            }
            OnHit(action);
            yield break;
        }

        private void OnHit(RangedAttack action) {
            if (action.poolProjectile)
            {
                transform.parent = action.Pooler.transform;
                gameObject.SetActive(false);
            }
            else
                Destroy(gameObject);
        }
    }
}
