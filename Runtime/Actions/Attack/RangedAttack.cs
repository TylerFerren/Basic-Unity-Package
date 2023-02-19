using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using Unity.VisualScripting;

namespace Codesign {
    public class RangedAttack : Attack
{
    protected enum RangedType {Raycast, Projectile}

    [Title("Ranged Settings")]
    [SerializeField] protected RangedType rangedType;
    [SerializeField, ShowIf("rangedType", RangedType.Projectile)] protected GameObject projectile;
    [SerializeField] protected LevelingValue<float> AttackRange = 3;
    [SerializeField] protected Vector3 firePoint;

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

    private void Start()
    {
        cam = Camera.main;
        if (ammo.Enabled) onFire.AddListener(ammo.UseAmmo);
        if (overheat.Enabled) onFire.AddListener(() => StartCoroutine(overheat.HeatUp(overheat.heatBuildUp / fireRate)));

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


        if (overheat.heatCooldown != null)
            StopCoroutine(overheat.heatCooldown);

        while (IsActive)
        {
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

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, AttackRange, attackableLayers, QueryTriggerInteraction.Ignore))
            targetPosition = hit.point;
        else
            targetPosition = cam.transform.position + cam.transform.forward * AttackRange;

        if(useSpread)
            targetOffset = spread * spreadRate.Evaluate(fireTimer) * Random.insideUnitSphere;

        targetPosition += targetOffset;
    }

    public void LinecastShot() {
        Debug.DrawLine(origin, targetPosition, Color.HSVToRGB(Random.Range(0.01f, 0.99f), 1, 1), 2);
        if (Physics.Linecast(origin, targetPosition, out RaycastHit hit, attackableLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.gameObject.TryGetComponent(out Health health))
            {
                Hit(hit.collider, health);
            }
        }
    }

    public void ProjectileShot() {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.TransformPoint(firePoint), 0.1f);
    }

}
}
