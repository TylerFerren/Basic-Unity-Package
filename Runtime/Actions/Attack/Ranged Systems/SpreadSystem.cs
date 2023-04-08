using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public class SpreadSystem : ActionSystem
    {
        [SerializeField, Tooltip("Maxium Radius of the spread")] protected LevelingValue<float> spread = 1;
        [SerializeField] protected AnimationCurve spreadRate = new AnimationCurve();
        [SerializeField] protected float spreadTime = 1;
        public float SpreadTime { get { return spreadTime; } }

        public Vector3 SpreadCalc(float time) {
            return spread * spreadRate.Evaluate(time/spreadTime) * Random.insideUnitSphere;
        }
    }
}
