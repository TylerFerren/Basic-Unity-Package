using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[System.Serializable, Toggle("Enabled")]
public class ChargeSystem
{
    public bool Enabled;
    [field: SerializeField] public float chargeRate { get; private set; }
    [field: SerializeField] public bool charageOnce { get; private set; }
    private float chargeTimer;

    [SerializeField, FoldoutGroup("Events")] protected UnityEvent onChargeStart;
    [SerializeField, FoldoutGroup("Events")] protected UnityEvent onChargeFinish;

    public IEnumerator Charge()
    {
        chargeTimer = 0;
        onChargeStart?.Invoke();
        while (chargeTimer < chargeRate)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }
        onChargeFinish?.Invoke();
    }

}
