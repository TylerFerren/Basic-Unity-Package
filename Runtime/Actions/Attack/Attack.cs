using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Codesign
{
    public abstract class Attack : Ability
    {
        public enum AttackTargetingType { Perspective, ForwardDirection, MousePosition, AutomaticTargeting, None }
        public enum AttackCriticalType { Chance, Spot }

        [Title("Attack Settings")]
        [SerializeField] protected LayerMask attackableLayers;
        public LayerMask AttackableLayers { get { return attackableLayers; } set { attackableLayers = value; } }

        [SerializeField, FoldoutGroup("Damage")] protected LevelingValue<float> damage = 10;
        public float Damage { get { return damage; } set { damage = value; } }
        [field: SerializeField, FoldoutGroup("Damage")] public AttackCriticalType CriticalType { get; set; }
        [field: SerializeField, FoldoutGroup("Damage"), ShowIf("CriticalType", AttackCriticalType.Chance), Range(0,1)] public float CriticalChance { get; set; }
        [field: SerializeField, FoldoutGroup("Damage")] public LevelingValue<float> CriticalDamage { get; set; } = 20;

        [SerializeField] protected LevelingValue<float> attackRange = 3;
        public float AttackRange { get { return attackRange; } set { attackRange = value; } }

        [field: SerializeField] public AttackTargetingType targetingType { get; set; } = AttackTargetingType.Perspective;
        [SerializeField, ShowIf("targetingType", AttackTargetingType.AutomaticTargeting)] protected AutomaticTargeting autoTargeting;


        public Collider TargetedObject { get; set; }

        [field: SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<Collider> OnHit = new ();
        [field: SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<float> OnDamage = new ();
        [field: SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<Collider> OnCriticalHit = new();
        [field: SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent<Collider> OnKill = new();

        public List<HitInfo> Hits { get; private set;} = new List<HitInfo>();

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

            if (CriticalType == AttackCriticalType.Spot && damageable is Health health && health.CriticalCollider.Contains(hit))
            {
                health.Damage(CriticalDamage);
                OnDamage?.Invoke(CriticalDamage);
                OnCriticalHit?.Invoke(hit);
                critical = true;
            }
            else if (CriticalType == AttackCriticalType.Chance && Random.value < CriticalChance) {
                damageable.Damage(CriticalDamage);
                OnDamage?.Invoke(CriticalDamage);
                OnCriticalHit?.Invoke(hit);
                critical = true;
            }
            else
            {
                damageable.Damage(damage);
                OnDamage?.Invoke(damage);
                OnHit?.Invoke(hit);
            }

            bool kill = false;
            if (damageable.CurrentValue <= 0)
            {
                OnKill?.Invoke(hit);
                kill = true;
            }
            Hits.Add(new HitInfo(this, hit, critical, kill));
        }

        public virtual void Hit(Collider hit, float distanceRatio) {

            distanceRatio = (distanceRatio + 1) / 2;
            var damageable = hit.transform.gameObject.GetComponentInChildren<IDamageable>();
            if (damageable == null) return;

            damageable.Damage(damage * distanceRatio);
            OnDamage?.Invoke(damage * distanceRatio);
            OnHit?.Invoke(hit);

            Debug.Log(damage * distanceRatio);

            bool kill = false;
            if (damageable.CurrentValue <= 0)
            {
                OnKill?.Invoke(hit);
                kill = true;
            }
            Hits.Add(new HitInfo(this, hit, false, kill));
        }
    }
}