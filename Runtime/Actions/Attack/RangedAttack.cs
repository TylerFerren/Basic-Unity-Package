using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor;

namespace Codesign {
    public class RangedAttack : Attack
    {
        protected enum RangedTargetingType {First_ThirdPerson, MousePosition, AutomaticTargeting}
        protected enum RangedType {Raycast, Projectile}

        [Title("Ranged Settings")]
        [SerializeField] protected RangedType rangedType;
        [SerializeField, ShowIf("rangedType", "RangedType.Projectile")] protected GameObject projectile;
        [SerializeField] protected LevelingValue<float> AttackRange = 3;
        [SerializeField, Tooltip("the origin of the Ranged Attack")] protected Vector3 firePoint;

        [SerializeField] protected RangedTargetingType targetingType = RangedTargetingType.First_ThirdPerson;

        [SerializeField, FoldoutGroup("FireRate")] protected LevelingValue<float> fireRate;
        [SerializeField, FoldoutGroup("FireRate")] protected bool continuousFire;

        [SerializeField, ToggleGroup("useSpread")] protected bool useSpread;
        [SerializeField, ToggleGroup("useSpread")] protected LevelingValue<float> spread;
        [SerializeField, ToggleGroup("useSpread")] protected AnimationCurve spreadRate = new AnimationCurve();

        [SerializeField] protected OverheatSystem overheat = new OverheatSystem();

        [SerializeField] protected AmmoSystem ammo = new AmmoSystem();

        [SerializeField] protected ChargeSystem charge = new ChargeSystem();

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent onFire;

        private float fireTimer;
        private Camera cam;
        private Vector3 origin;
        private Vector3 targetPosition;
        private Vector3 targetOffset;
        [ReadOnly] public Collider targetedObject;

        private void Awake()
        {
            if (targetingType != RangedTargetingType.AutomaticTargeting) cam = Camera.main;
            if (ammo.Enabled) onFire.AddListener(ammo.UseAmmo);
            if (overheat.Enabled) onFire.AddListener(() => StartCoroutine(overheat.HeatUp(overheat.heatBuildUp / fireRate)));
            AutomaticIsActive = false;
            StartCoroutine(AutomaticTargeting());
        }

        private IEnumerator AutomaticTargeting() {
            while (targetingType == RangedTargetingType.AutomaticTargeting) {
                var targetOptions = Physics.OverlapSphere(transform.position, AttackRange, attackableLayers);
                if (targetOptions.FirstOrDefault())
                {
                    AutomaticIsActive = true;
                    targetedObject = targetOptions.FirstOrDefault();
                }
                else if(AutomaticIsActive)
                {
                    IsActive = false;
                    AutomaticIsActive = false;
                }
                yield return new WaitForFixedUpdate();
            }
        }

        public override void Trigger(InputAction.CallbackContext context)
        {
            if (ActiveCooldown != null) return;
            if (overheat.isOverheated) return;
            base.Trigger(context);
            StartCoroutine(Fire());
        }

        public override void Release(InputAction.CallbackContext context)
        {
            base.Release(context);
        }

        public IEnumerator Fire()
        {
            fireTimer = 0;

            if (overheat.heatCooldown != null) StopCoroutine(overheat.heatCooldown);

            while (IsActive)
            {

                if (triggerType == ActionTriggerType.Automatic && !AutomaticIsActive)
                {
                    print("Auto stop");
                    yield break;
                }
                if (ammo.Enabled && ammo.currentAmmo <= 0)
                    yield return null;

                if (charge.Enabled && charge.charageOnce)
                    yield return StartCoroutine(charge.Charge());

                ShotCalculation();

                switch (rangedType) {
                    case RangedType.Projectile:
                        ProjectileShot();
                        break;
                    case RangedType.Raycast:
                        LinecastShot();
                        break;
                }
            
                fireTimer += 1 / fireRate;

                onFire?.Invoke();

                if (!continuousFire || overheat.currentHeat > overheat.HeatLimit) break;

                yield return new WaitForSeconds(1 / fireRate);
            }

            if (overheat.Enabled && overheat.currentHeat > 0)
                overheat.heatCooldown = StartCoroutine(overheat.HeatCooldown());

            ActiveCooldown = StartCoroutine(useCooldown ? Cooldown() : Cooldown(1 / fireRate));
        }


        public void ShotCalculation() {
            origin = transform.TransformPoint(firePoint);

            switch (targetingType) {
                case RangedTargetingType.First_ThirdPerson:
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, AttackRange, attackableLayers, QueryTriggerInteraction.Ignore))
                        targetPosition = hit.point;
                    else
                        targetPosition = cam.transform.position + cam.transform.forward * AttackRange;
                    break;
                case RangedTargetingType.MousePosition:
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    targetPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.nearClipPlane));
                    break;
                case RangedTargetingType.AutomaticTargeting:
                    if(targetedObject) targetPosition = targetedObject.bounds.center;
                    break;

            }
            if(useSpread)
                targetOffset = spread * spreadRate.Evaluate(fireTimer) * Random.insideUnitSphere;

            targetPosition += targetOffset;
        }

        public void LinecastShot() {
            Debug.DrawLine(origin, targetPosition, Color.HSVToRGB(Random.Range(0.01f, 0.99f), 1, 1), 2);
            if (Physics.Linecast(origin, targetPosition, out RaycastHit hit, attackableLayers, QueryTriggerInteraction.Ignore))
            {
                var health = hit.transform.gameObject.GetComponent<Health>();

                Hit(hit.collider, health);
            }
        }

        public void ProjectileShot() {

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.TransformPoint(firePoint), 0.1f);
            Handles.DrawWireDisc(transform.position, transform.up, AttackRange);
        }

    }
}
