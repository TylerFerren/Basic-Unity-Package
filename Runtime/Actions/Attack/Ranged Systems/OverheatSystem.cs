using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public class OverheatSystem
    {
        public bool Enabled;
        [field: SerializeField, ProgressBar(0, "HeatLimit", 0.8f, 0.2f, 0.2f)] public float currentHeat { get; set; } = 0;
        [field: SerializeField] public LevelingValue<float> HeatLimit { get; set; } = 100;
        [field: SerializeField] public LevelingValue<float> heatBuildUp { get; set; } = 3;
        [field: SerializeField] public LevelingValue<float> heatDisapation { get; set; } = 5;
        [field: SerializeField] public LevelingValue<float> cooledDownLimit { get; set; } = 20;
        [field: SerializeField] public LevelingValue<float> HeatCooldownDelay { get; set; } = 1;
        [field: SerializeField] public bool isOverheated { get; set; }
        public Coroutine heatCooldown;

        public IEnumerator HeatUp(float heat)
        {
            var newHeat = currentHeat + heat;
            while (currentHeat < newHeat)
            {
                currentHeat += heatBuildUp * Time.deltaTime;
                yield return null;
            }
        }

        public IEnumerator HeatCooldown()
        {

            if (currentHeat > HeatLimit)
            {
                currentHeat = HeatLimit;
                isOverheated = true;
            }

            yield return new WaitForSeconds(HeatCooldownDelay);

            while (currentHeat > 0)
            {
                if (currentHeat < cooledDownLimit) isOverheated = false;
                currentHeat -= heatDisapation * Time.deltaTime;
                yield return null;
            }

            currentHeat = 0;
            isOverheated = false;

            yield return null;
        }
    }

}