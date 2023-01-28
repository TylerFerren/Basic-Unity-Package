using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public abstract class Attack : Action
{
    [Title("Attack Settings")]
    [SerializeField] protected float damage;
    [SerializeField] protected float CriticalDamage;

    [SerializeField] protected LayerMask attackableLayers;

    [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnHit;
    [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnCriticalHit;
    [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnKill;

    public override void Trigger(InputAction.CallbackContext context)
    {
        base.Trigger(context);
    }

    public override void Release(InputAction.CallbackContext context)
    {
        base.Release(context);
    }

    public virtual void Hit(Collider collider, Health health) {
        if (health.CriticalCollider.Contains(collider))
        {
            health.Damage(CriticalDamage);
            OnCriticalHit?.Invoke(collider);
        }
        else {
            health.Damage(damage);
            OnHit?.Invoke(collider);
        }

        if (health.CurrentValue <= 0) OnKill?.Invoke(collider);
    }

}
