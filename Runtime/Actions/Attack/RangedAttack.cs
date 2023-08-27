using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEditor;

namespace Codesign {
    public class RangedAttack : Attack
    {
        public enum RangedType {Raycast, Projectile}
        public enum FireType { Single, Burst, Continuous}

        [Title("Ranged Settings")]
        public RangedType rangedType;
        [ShowIf("rangedType", RangedType.Projectile)] public GameObject projectile;
        [SerializeField, ShowIf("rangedType", RangedType.Projectile)] public bool poolProjectile;
        [SerializeField, ShowIf("poolProjectile")] protected ObjectPooler pooler;
        public ObjectPooler Pooler { get { return pooler; } set { Pooler = value; } }

        [SerializeField, Tooltip("The Origin of the Ranged Attack")] protected Vector3 firePoint;
        public Vector3 FirePoint { get { return firePoint; } set { firePoint = value; } }


        [field: SerializeField, FoldoutGroup("Fire Rate")] public FireType fireType { get; set; } = FireType.Single;
        [SerializeField, FoldoutGroup("Fire Rate")] protected LevelingValue<float> fireRate = 2;
        public float FireRate { get { return fireRate; } set { fireRate = value; } }
        [field: SerializeField, FoldoutGroup("Fire Rate"), ShowIf("fireType", FireType.Burst)] public float BurstAmount { get; set; } = 3;
        [field: SerializeField, FoldoutGroup("Fire Rate"), ShowIf("fireType", FireType.Burst)] public float BurstSpacing { get; set; } = 0.1f;



        [SerializeField] protected SpreadSystem spread = new SpreadSystem();
        public SpreadSystem Spread { get { return spread; } set { spread = value; } }

        [SerializeField] protected OverheatSystem overheat = new OverheatSystem();

        [SerializeField] protected AmmoSystem ammo = new AmmoSystem();

        [SerializeField] protected RecoilSystem recoil = new RecoilSystem();

        [FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent OnFire;
        [SerializeField, FoldoutGroup("Events"), PropertyOrder(99)] protected UnityEvent<Vector3, Vector3> onFirePositions;

        private float spreadTime;
        private float lastFireTime;
        private Camera cam;
        private Vector3 origin;
        private Vector3 targetPosition;
        private Vector3 targetOffset;
        public Vector3 FireDestination { get; private set; }

        private void OnValidate()
        {
            if (rangedType != RangedType.Projectile) poolProjectile = false;
        }

        public override void Awake()
        {
            base.Awake();
            if (targetingType != AttackTargetingType.AutomaticTargeting) cam = Camera.main;
            if (ammo.Enabled) OnFire.AddListener(ammo.UseAmmo);
            if (overheat.Enabled) OnFire.AddListener(() => StartCoroutine(overheat.HeatUp(overheat.heatBuildUp / fireRate)));
            if (recoil.Enabled) OnFire.AddListener(recoil.Recoil);
            if (poolProjectile && pooler == null) pooler = GetComponent<ObjectPooler>();
            if (poolProjectile && pooler && !pooler.ObjectsToPool.Exists(n => n.objectToPool == projectile)) {
                pooler.ObjectsToPool.Add(new ObjectPoolItem(projectile, 5, true, pooler));
            }
        }

        public override IEnumerator Trigger()
        {
            yield return StartCoroutine(base.Trigger());

            if ((ammo.Enabled && ammo.useMagazine && ammo.MagazineAmount <= 0) || (ammo.Enabled && ammo.currentAmmo <= 0))
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

            int burstCount = 0;
            do
            {
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

                OnFire?.Invoke();
                onFirePositions?.Invoke(transform.TransformPoint(firePoint), targetPosition);

                if (Time.time - lastFireTime < spread.SpreadTime) spreadTime = (Time.time - lastFireTime) / spread.SpreadTime;
                spreadTime += 1 / fireRate;

                if (fireType == FireType.Burst) {
                    burstCount++;
                    yield return new WaitForSeconds(BurstSpacing);
                }
            } while (fireType == FireType.Burst & burstCount < BurstAmount);

            yield return new WaitForSeconds(1 / fireRate);

            if (IsActive && fireType == FireType.Continuous ) yield return StartCoroutine(Trigger());
        }

        public override IEnumerator Finish()
        {
            spreadTime = 0;
            lastFireTime = Time.time;

            if (overheat.Enabled && overheat.currentHeat > 0)
                overheat.heatCooldown = StartCoroutine(overheat.HeatCooldown());

            yield return StartCoroutine(base.Finish());
        }


        public void ShotCalculation() {
            origin = transform.TransformPoint(firePoint);

            switch (targetingType) {
                case AttackTargetingType.Perspective:
                    if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackRange, attackableLayers, QueryTriggerInteraction.Ignore))
                        targetPosition = hit.point;
                    else
                        targetPosition = cam.transform.position + cam.transform.forward * attackRange;
                    break;
                case AttackTargetingType.ForwardDirection:
                    targetPosition = origin + (transform.forward * attackRange);
                    break;
                case AttackTargetingType.MousePosition:
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    new Plane(transform.up, origin).Raycast(ray, out float enter);
                    Vector3 targetVector = (ray.GetPoint(enter) - (transform.position + new Vector3(0,firePoint.y,0)));
                    targetPosition = origin + Vector3.ClampMagnitude(targetVector, attackRange);
                    break;
                case AttackTargetingType.AutomaticTargeting:
                    if(TargetedObject) targetPosition = TargetedObject.bounds.center;
                    break;
            }
            if(spread.Enabled)
                targetOffset = spread.SpreadCalc(spreadTime);

            targetPosition += targetOffset;
        }

        public void LinecastShot() {
            Debug.DrawLine(origin, targetPosition, Color.HSVToRGB(Random.Range(0.01f, 0.99f), 1, 1), 2);

            if (Physics.Linecast(origin, targetPosition, out RaycastHit hit, attackableLayers, QueryTriggerInteraction.Ignore))
            {
                Hit(hit.collider);
                FireDestination = hit.point;
            }
            else FireDestination = targetPosition;
        }

        public void ProjectileShot()
        {
            GameObject bullet = null;
            if (poolProjectile && pooler)
            {
                bullet = pooler.GetPooledObject(projectile);
                bullet.transform.parent = null;
                bullet.transform.position = transform.TransformPoint(firePoint);
                bullet.transform.LookAt(targetPosition);
            }
            if (bullet == null)
            {
                if (poolProjectile) print("no pooled object availible");
                bullet = Instantiate(projectile, transform.TransformPoint(firePoint), transform.rotation, null);
                bullet.transform.LookAt(targetPosition);
            }
            if (bullet.TryGetComponent(out Projectile _projectile))
            {
                StartCoroutine(_projectile.Fire(targetPosition, this, transform.root.gameObject));
            }
            else Debug.LogWarning("Projectile prefab must have Projectile componet on it.");
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawSphere(transform.TransformPoint(firePoint), 0.05f);
            Gizmos.DrawSphere(targetPosition, 0.01f);
            Handles.DrawWireDisc(transform.TransformPoint(firePoint), transform.up, attackRange);
        }

    }
}
