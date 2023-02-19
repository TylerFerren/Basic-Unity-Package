using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codesign
{
    [RequireComponent(typeof(Collider))]
    public class MeleeAttack : Attack
    {
        [SerializeField] private Collider hitBox;
        [SerializeField, FoldoutGroup("Attack Timing")] private LevelingValue<float> attackDelay;
        [SerializeField, FoldoutGroup("Attack Timing")] private LevelingValue<float> attackDuration;

        private float attackTimer;

        void Start()
        {
            if (hitBox == null) hitBox = GetComponent<Collider>();
            hitBox.enabled = false;
        }

        public override void Trigger(InputAction.CallbackContext context)
        {
            if (ActiveCooldown != null) return;
            base.Trigger(context);
            StartCoroutine(Attack());
        }

        public override void Release(InputAction.CallbackContext context)
        {
            base.Release(context);
        }

        public IEnumerator Attack()
        {
            yield return new WaitForSeconds(attackDelay);
            hitBox.enabled = true;

            List<Collider> hits = new List<Collider>();

            attackTimer = 0;
            while (attackTimer < attackDuration)
            {
                var framehitBoxes = Physics.OverlapSphere(hitBox.bounds.center, hitBox.bounds.extents.magnitude);
                foreach (Collider collider in framehitBoxes)
                {
                    if (!hits.Contains(collider) && !collider.transform.IsChildOf(transform))
                    {
                        hits.Add(collider);
                        if (collider.TryGetComponent(out Health health))
                        {
                            health.Damage(damage);
                        }
                    }
                }
                attackTimer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(attackDuration);
            hitBox.enabled = false;
        }
    }

}