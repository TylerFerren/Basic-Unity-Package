using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    [System.Serializable, Toggle("Enabled")]
    public class AutomaticRefill
    {
        public bool Enabled;
        [SerializeField] private float refillRate = 3;
        [SerializeField] private float refillDelay = 3;

        public IEnumerator Refill(Status status)
        {
            yield return new WaitForSeconds(refillDelay);
            while (status.CurrentValue < status.MaxValue)
            {
                status.Refill(refillRate * Time.fixedDeltaTime);
                yield return null;
            }
        }
    }
}
