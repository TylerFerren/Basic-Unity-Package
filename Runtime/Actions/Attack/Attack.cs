using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Codesign
{
    public abstract class Attack : Action
    {
        [Title("Attack Settings")]
        [SerializeField] protected LevelingValue<float> damage = 10;
        [SerializeField] protected LevelingValue<float> criticalDamage;

        [SerializeField] protected LayerMask attackableLayers;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnHit;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnCriticalHit;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnKill;

        [SerializeField, ReadOnly] private List<HitInfo> hits = new List<HitInfo>();

        public override void Trigger(InputAction.CallbackContext context)
        {
            base.Trigger(context);
        }

        public override void Release(InputAction.CallbackContext context)
        {
            base.Release(context);
        }

        public virtual void Hit(Collider collider, Health health)
        {
            if (health.CriticalCollider.Contains(collider))
            {
                health.Damage(criticalDamage);
                OnCriticalHit?.Invoke(collider);
            }
            else
            {
                health.Damage(damage);
                OnHit?.Invoke(collider);
            }

            bool kill = false;
            if (health.CurrentValue <= 0)
            {
                OnKill?.Invoke(collider);
                kill = true;
            }

            hits.Add(new HitInfo(this, collider, kill));
        }

        public void UpdateAttack(float _damage, float _criticalDamage)
        {
            damage = _damage;
            criticalDamage = _criticalDamage;
        }

    }

}