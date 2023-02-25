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
    public abstract class Attack : Action
    {
        protected enum AttackTargetingType { First_ThirdPerson, MousePosition, AutomaticTargeting, None }

        [Title("Attack Settings")]
        [SerializeField] protected LayerMask attackableLayers;

        [SerializeField, FoldoutGroup("Damage")] protected LevelingValue<float> damage = 10;
        public void LevelUpDamage() => damage.LevelUp();
        [SerializeField, FoldoutGroup("Damage")] protected LevelingValue<float> criticalDamage = 20;
        public void LevelUpCriticalDamage() => criticalDamage.LevelUp();

        [SerializeField] protected LevelingValue<float> AttackRange = 3;

        [SerializeField] protected AttackTargetingType targetingType = AttackTargetingType.First_ThirdPerson;

        [ReadOnly] public Collider targetedObject;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnHit;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnCriticalHit;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<Collider> OnKill;

        public List<HitInfo> hits { get; private set;} = new List<HitInfo>();

        public override void OnEnable()
        {
            base.OnEnable();
            if(targetingType == AttackTargetingType.AutomaticTargeting)
                    StartCoroutine(AutomaticTargeting());
        }

        public override IEnumerator Trigger()
        {
            yield return StartCoroutine(base.Trigger());
        }

        public override IEnumerator Release()
        {
            yield return StartCoroutine(base.Release());
        }

        public virtual void Hit(Collider collider, Health health)
        {
            
            if (!health) return;
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

        protected IEnumerator AutomaticTargeting()
        {
            //Searches for a list of targets every fixed update
            while (targetingType == AttackTargetingType.AutomaticTargeting)
            {
                var targetOptions = Physics.OverlapSphere(transform.position, AttackRange, attackableLayers);

                targetOptions.Sort((a, b) =>
                {
                    float distA = Vector3.Distance(transform.position, a.transform.position);
                    float distB = Vector3.Distance(transform.position, b.transform.position);
                    return distA.CompareTo(distB);
                });

                if (targetOptions.FirstOrDefault())
                {
                    AutomaticIsActive = true;
                    targetedObject = targetOptions.FirstOrDefault();
                }
                else if (AutomaticIsActive)
                {
                    IsActive = false;
                    AutomaticIsActive = false;
                }
                yield return new WaitForFixedUpdate();
            }
        }
    }
}