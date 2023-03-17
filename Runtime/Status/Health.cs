using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections.Generic;


namespace Codesign
{
    public class Health : Status, IDamageable
    {
        [field: SerializeField] public List<Collider> CriticalCollider { get; private set; } = new List<Collider>();
        private bool isAlive = true;
        [FoldoutGroup("Event")] public UnityEvent OnBirth = new UnityEvent();
        [FoldoutGroup("Event")] public UnityEvent OnDeath = new UnityEvent();

        public new void OnEnable()
        {
            base.OnEnable();
            OnBirth.Invoke();
        }
        
        public void Reset()
        {
            inspectorBarColor = new Color(0.5f, 0.9f, 0.6f, 1);
            currentValue = MaxValue / 2;
        }

        public void Damage(float amount)
        {
            AdjustStatus(-amount);

            if (CurrentValue <= 0)
            {
                isAlive = false;
                OnDeath.Invoke();
            }
        }

        public bool IsAlive()
        {
            return isAlive;
        }
    }
}
