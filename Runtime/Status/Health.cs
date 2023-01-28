using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections.Generic;
using Codesign;

public class Health : Status, IDamageable
{
    [field: SerializeField] public List<Collider> CriticalCollider { get; private set; } = new List<Collider>();
    [SerializeField] private float CriticalMultiplier = 2;

    [FoldoutGroup("Event")] public UnityEvent OnBirth = new UnityEvent();
    [FoldoutGroup("Event")] public UnityEvent OnDeath = new UnityEvent();

    public new void OnEnable()
    {
        base.OnEnable();
        OnBirth.Invoke();
    }

    public void OnValidate()
    {
        InspectorBarColor = new Color(0.5f, 0.9f, 0.6f, 1);
    }

    public void Damage(float damage)
    {
        AdjustStatus(-damage);

        if (CurrentValue <= 0) {
            OnDeath.Invoke();
        }
    }

}
