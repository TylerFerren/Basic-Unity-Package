using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    [System.Serializable, Toggle("Enabled")]
    public class AutomaticUpdate
    {
        public bool Enabled;
        [field: SerializeField] public float UpdateRate { get; set; } = 1; 
        [field: SerializeField] public float UpdateDelay { get; set; } = 1;

        public IEnumerator Refill(Status status)
        {
            yield return new WaitForSeconds(UpdateDelay);
            while (status.CurrentValue < status.MaxValue)
            {
                status.Refill(UpdateRate * Time.fixedDeltaTime);
                yield return null;
            }
        }
    }
}
