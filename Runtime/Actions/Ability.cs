using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    public abstract class Ability : Action
    {
        [Title("Ability Settings")]
        [SerializeField] protected CooldownSystem cooldown = new CooldownSystem();
        [SerializeField] protected ChargeSystem charge = new ChargeSystem();

        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected bool useStatus;
        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected Status status;


        public override IEnumerator Trigger()
        {
            if (cooldown.Enabled && cooldown.ActiveCooldown != null) yield break;

            if (charge.Enabled)
            {
                if (charge.charageOnce && !IsActive || !charge.charageOnce)
                {
                    IsActive = true;
                    yield return StartCoroutine(charge.Charge());
                }

                if (!IsActive) yield break;
            }
            else IsActive = true;

            yield return base.Trigger();
        }


        public override IEnumerator Finish()
        {
            if (cooldown.Enabled && cooldown.ActiveCooldown != null) cooldown.ActiveCooldown = StartCoroutine(cooldown.Cooldown());
            return base.Finish();
        }
    }

}
