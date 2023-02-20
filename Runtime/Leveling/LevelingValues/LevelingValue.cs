using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Codesign
{
    [Serializable]
    public class LevelingValue<T>
    {
        public LevelingValue(T value)
        {
            Value = value;
            if (typeof(T) == typeof(float))
            {
                curve = new EvaluationCurve((float)(object)value, 1, 1);
            }
            else if (typeof(T) == typeof(int))
            {
                curve = new EvaluationCurve((int)(object)value, 1, 1);
            }
        }

        public LevelingValue(T value, float floor, float linear, float exponetial)
        {
            Value = value;
            curve = new EvaluationCurve(floor, linear, exponetial);
        }

        public static implicit operator LevelingValue<T>(T value)
        {
            return new LevelingValue<T>(value);
        }

        public static implicit operator T(LevelingValue<T> levelingValue)
        {
            return levelingValue.Value;
        }

        public T Value;
        public int Level = 0;
        [SerializeField]
        public EvaluationCurve curve = new EvaluationCurve();

        public void LevelUp()
        {
            Level++;
            if (typeof(T) == typeof(float))
            {
                Value = (T)(object)curve.Evaluate(Level);
            }
            else if (typeof(T) == typeof(int))
            {
                Value = (T)(object)curve.EvaluateInt(Level);
            }
        }

        public static void LevelUp(LevelingValue<T> levelingValue)
        {
            levelingValue.Level++;
            if (typeof(T) == typeof(float))
            {
                levelingValue.Value = (T)(object)levelingValue.curve.Evaluate(levelingValue.Level);
            }
            else if (typeof(T) == typeof(int))
            {
                levelingValue.Value = (T)(object)levelingValue.curve.EvaluateInt(levelingValue.Level);
            }
        }
    }
}