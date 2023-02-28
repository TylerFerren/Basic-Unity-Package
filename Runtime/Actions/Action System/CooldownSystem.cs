using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    [System.Serializable, Toggle("Enabled")]
    public class CooldownSystem : ActionSystem
    {
        [SerializeField] public LevelingValue<float> cooldownTime = new LevelingValue<float>(3, 3, 0.66f, 1);
        public void LevelUpCooldownTime() => cooldownTime.LevelUp();
        [SerializeField, ProgressBar(0, "cooldownTime", r: 1, g: 1, b: 1, Height = 20)] protected float CurrentTimer;
        public Coroutine ActiveCooldown;

        public IEnumerator Cooldown()
        {
            CurrentTimer = cooldownTime;
            while (CurrentTimer > 0)
            {
                CurrentTimer -= Time.deltaTime;
                yield return null;
            }
            CurrentTimer = 0;
            ActiveCooldown = null;
        }

        public IEnumerator Cooldown(float cooldownTime)
        {
            CurrentTimer = cooldownTime;
            while (CurrentTimer > 0)
            {
                CurrentTimer -= Time.deltaTime;
                yield return null;
            }
            CurrentTimer = 0;
            ActiveCooldown = null;
        }
    }
}
