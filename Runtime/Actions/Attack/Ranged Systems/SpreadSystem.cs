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
        public float Spread { get { return spread;} set { spread = value;} }
        [SerializeField] protected AnimationCurve spreadRate = new AnimationCurve();
        public AnimationCurve SpreadRate { get { return spreadRate; } set { spreadRate = value; } }
        [SerializeField] protected float spreadTime = 1;
        public float SpreadTime { get { return spreadTime; } }

        [field: SerializeField] public Vector3 SpreadDirections;

        public Vector3 SpreadCalc(float time) {
            return spread * spreadRate.Evaluate(time/spreadTime) *
                new Vector3(Random.insideUnitSphere.x * SpreadDirections.x, Random.insideUnitSphere.y * SpreadDirections.y, Random.insideUnitSphere.z * SpreadDirections.z);
        }
    }
}
