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
using UnityEngine.SocialPlatforms;

namespace Codesign {
    public class RangedAttack : Attack
    {
        
        protected enum RangedType {Raycast, Projectile}

        [Title("Ranged Settings")]
        [SerializeField] protected RangedType rangedType;
        [SerializeField, ShowIf("rangedType", RangedType.Projectile)] protected GameObject projectile;
        [SerializeField, ShowIf("rangedType", RangedType.Projectile)] public bool poolProjectile;
        [SerializeField, ShowIf("poolProjectile")] protected ObjectPooler pooler;
        public ObjectPooler Pooler { get { return pooler; } }

        [SerializeField, Tooltip("the origin of the Ranged Attack")] protected Vector3 firePoint;

        [SerializeField, FoldoutGroup("FireRate")] protected LevelingValue<float> fireRate = 2;
        [SerializeField, FoldoutGroup("FireRate")] protected bool continuousFire;

        [SerializeField, ToggleGroup("useSpread")] protected bool useSpread;
        [SerializeField, ToggleGroup("useSpread")] protected LevelingValue<float> spread = 1;
        [SerializeField, ToggleGroup("useSpread")] protected AnimationCurve spreadRate = new AnimationCurve();

        [SerializeField] protected OverheatSystem overheat = new OverheatSystem();

        [SerializeField] protected AmmoSystem ammo = new AmmoSystem();

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent onFire;

        private float fireTimer;
        private Camera cam;
        private Vector3 origin;
        private Vector3 targetPosition;
        private Vector3 targetOffset;

        private void OnValidate()
        {
            if (rangedType != RangedType.Projectile) poolProjectile = false;
        }

        private void Awake()
        {
            if (targetingType != AttackTargetingType.AutomaticTargeting) cam = Camera.main;
            if (ammo.Enabled) onFire.AddListener(ammo.UseAmmo);
            if (overheat.Enabled) onFire.AddListener(() => StartCoroutine(overheat.HeatUp(overheat.heatBuildUp / fireRate)));
            if (poolProjectile && pooler && pooler.ObjectsToPool.Find(n => n.objectToPool == projectile) == null) {
                pooler.ObjectsToPool.Add(new ObjectPoolItem(projectile, 5, true, pooler));
            }
        }

        public override IEnumerator Trigger()
        {
            yield return StartCoroutine(base.Trigger());

            if (ammo.Enabled && ammo.currentAmmo <= 0)
            {
                StartCoroutine(Finish());
                yield break;
            }

            if (overheat.Enabled && overheat.isOverheated)
            {
                StartCoroutine(Finish());
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

            if (IsActive && continuousFire ) yield return StartCoroutine(Trigger());
        }

        public override IEnumerator Finish()
        {
            fireTimer = 0;

            if (overheat.Enabled && overheat.currentHeat > 0)
                overheat.heatCooldown = StartCoroutine(overheat.HeatCooldown());

            yield return StartCoroutine(base.Finish());
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
                    Plane attackHeightPlane = new Plane(transform.up, firePoint);
                    attackHeightPlane.Raycast(cam.ScreenPointToRay(mousePosition), out float distance);
                    targetPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));
                    targetPosition = Vector3.MoveTowards(transform.TransformPoint(firePoint), targetPosition, AttackRange);
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
                Hit(hit.collider);
            }
        }

        public void ProjectileShot()
        {
            GameObject bullet = null;
            if (poolProjectile && pooler)
            {
                bullet = pooler.GetPooledObject(projectile);
                bullet.transform.position = transform.TransformPoint(firePoint);
                bullet.transform.LookAt(targetPosition);
            }
            if (bullet == null) {
                if (poolProjectile) print("no pooled object availible");
                bullet = Instantiate(projectile, transform.TransformPoint(firePoint), transform.rotation, null);
                bullet.transform.LookAt(targetPosition);
            }
            if (bullet.TryGetComponent(out Projectile _projectile))
                StartCoroutine(_projectile.Fire(targetPosition, this));
            
            else Debug.LogWarning("Projectile prefab must have Projectile componet on it.");
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.2f, 0.2f, 0.33f);
            Gizmos.DrawSphere(transform.TransformPoint(firePoint), 0.05f);
            Handles.DrawWireDisc(transform.position, transform.up, AttackRange);
        }

    }
}
