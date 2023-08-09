using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Linq;
using Sirenix.Utilities;

namespace Codesign
{
    public abstract class Attack : Ability
    {
        protected enum AttackTargetingType { Perspective, ForwardDirection, MousePosition,  AutomaticTargeting, None }

        [Title("Attack Settings")]
        [SerializeField] protected LayerMask attackableLayers;
        public LayerMask AttackableLayers { get { return attackableLayers; } set { attackableLayers = value;} }

        [SerializeField, FoldoutGroup("Damage")] protected LevelingValue<float> damage = 10;
        public float Damage { get { return damage; } set { damage = value; } }

        [SerializeField, FoldoutGroup("Damage")] protected LevelingValue<float> criticalDamage = 20;

        [SerializeField] protected LevelingValue<float> attackRange = 3;
        public float AttackRange { get { return attackRange; } set { attackRange = value; } }

        [SerializeField] protected AttackTargetingType targetingType = AttackTargetingType.Perspective;
        [SerializeField, ShowIf("targetingType", AttackTargetingType.AutomaticTargeting)] protected AutomaticTargeting autoTargeting;


        public Collider TargetedObject { get; set; }

        [SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] protected UnityEvent<Collider> OnHit;
        [SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] protected UnityEvent<Collider> OnCriticalHit;
        [SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] protected UnityEvent<Collider> OnKill;

        public List<HitInfo> hits { get; private set;} = new List<HitInfo>();

        public virtual void Awake()
        {
            if (targetingType == AttackTargetingType.AutomaticTargeting && autoTargeting == null)
            {
                autoTargeting = GetComponent<AutomaticTargeting>();
            }
        }

        public override IEnumerator Trigger()
        {
            yield return StartCoroutine(base.Trigger());
            if (autoTargeting) TargetedObject = autoTargeting.TargetedObject;
        }

        public override IEnumerator Release()
        {
            yield return StartCoroutine(base.Release());
        }

        public virtual void Hit(Collider hit)
        {
            var damageable =  hit.transform.gameObject.GetComponentInChildren<IDamageable>();
            if (damageable == null) return;
            bool critical = false;
            if (damageable is Health health && health.CriticalCollider.Contains(hit))
            {
               health.Damage(criticalDamage);
                OnCriticalHit?.Invoke(hit);
                critical = true;
            }
            else
            {
                damageable.Damage(damage);
                OnHit?.Invoke(hit);
            }

            bool kill = false;
            if (damageable.CurrentValue <= 0)
            {
                OnKill?.Invoke(hit);
                kill = true;
            }
            hits.Add(new HitInfo(this, hit, critical, kill));

        }
    }
}