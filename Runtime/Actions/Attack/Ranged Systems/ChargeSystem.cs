using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public class ChargeSystem : ActionSystem
    {
        [field: SerializeField] public float chargeRate { get; private set; }
        [field: SerializeField] public bool charageOnce { get; private set; }
        private float chargeTimer;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent onChargeStart;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent<float> onChargeProgress;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent onChargeFinish;

        public IEnumerator Charge()
        {
            chargeTimer = 0;
            onChargeStart?.Invoke();
            onChargeProgress?.Invoke(0);
            while (chargeTimer < chargeRate)
            {
                onChargeProgress?.Invoke(chargeTimer / chargeRate);
                chargeTimer += Time.deltaTime;
                yield return null;
            }
            onChargeProgress?.Invoke(1);
            yield return null;
            onChargeFinish?.Invoke();
        }

    }
}