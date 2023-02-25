using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor;
using Sirenix.Utilities;

namespace Codesign {
    public class RangedAttack : Attack
    {
        
        protected enum RangedType {Raycast, Projectile}

        [Title("Ranged Settings")]
        [SerializeField] protected RangedType rangedType;
        [SerializeField, ShowIf("rangedType", "RangedType.Projectile")] protected GameObject projectile;

        [SerializeField, Tooltip("the origin of the Ranged Attack")] protected Vector3 firePoint;

        [SerializeField, FoldoutGroup("FireRate")] protected LevelingValue<float> fireRate;
        [SerializeField, FoldoutGroup("FireRate")] protected bool continuousFire;

        [SerializeField, ToggleGroup("useSpread")] protected bool useSpread;
        [SerializeField, ToggleGroup("useSpread")] protected LevelingValue<float> spread;
        [SerializeField, ToggleGroup("useSpread")] protected AnimationCurve spreadRate = new AnimationCurve();

        [SerializeField] protected OverheatSystem overheat = new OverheatSystem();

        [SerializeField] protected AmmoSystem ammo = new AmmoSystem();

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent onFire;

        private float fireTimer;
        private Camera cam;
        private Vector3 origin;
        private Vector3 targetPosition;
        private Vector3 targetOffset;

        private void Awake()
        {
            if (targetingType != AttackTargetingType.AutomaticTargeting) cam = Camera.main;
            if (ammo.Enabled) onFire.AddListener(ammo.UseAmmo);
            if (overheat.Enabled) onFire.AddListener(() => StartCoroutine(overheat.HeatUp(overheat.heatBuildUp / fireRate)));
        }

        public override IEnumerator Trigger()
        {
            yield return StartCoroutine(base.Trigger());

            if (triggerType == ActionTriggerType.Automatic && !AutomaticIsActive)
            {
                StartCoroutine(Release());
                yield break;
            }

            if (ammo.Enabled && ammo.currentAmmo <= 0)
            {
                StartCoroutine(Release());
                yield break;
            }

            if (overheat.Enabled && overheat.isOverheated)
            {
                StartCoroutine(Release());
                yield break;
            }

            if (overheat.Enabled && overheat.heatCooldown != null) StopCoroutine(overheat.heatCooldown);

            ShotCalculation();

            switch (rangedType)
            {
                case RangedType.Projectile:
                    ProjectileShot();
                    break;
                case RangedType.Raycast:
                    LinecastShot();
                    break;
            }

            onFire?.Invoke();

            fireTimer += 1 / fireRate;

            yield return new WaitForSeconds(1 / fireRate);

            if (IsActive && continuousFire && triggerType != ActionTriggerType.Automatic) StartCoroutine(Trigger());
        }

        public override IEnumerator Release()
        {
            fireTimer = 0;

            if (overheat.Enabled && overheat.currentHeat > 0)
                overheat.heatCooldown = StartCoroutine(overheat.HeatCooldown());

            yield return StartCoroutine(base.Release());
        }


        public void ShotCalculation() {
            origin = transform.TransformPoint(firePoint);

            switch (targetingType) {
                case AttackTargetingType.First_ThirdPerson:
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, AttackRange, attackableLayers, QueryTriggerInteraction.Ignore))
                        targetPosition = hit.point;
                    else
                        targetPosition = cam.transform.position + cam.transform.forward * AttackRange;
                    break;
                case AttackTargetingType.MousePosition:
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    targetPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.nearClipPlane));
                    break;
                case AttackTargetingType.AutomaticTargeting:
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

        public void ProjectileShot()
        {

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.TransformPoint(firePoint), 0.1f);
            Handles.DrawWireDisc(transform.position, transform.up, AttackRange);
        }

    }
}
