using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

namespace Codesign
{
    [Serializable]
    public class LevelingValue<T> : ISerializationCallbackReceiver where T:struct, IConvertible, IComparable<T>, IEquatable<T>, IFormattable
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

        public LevelingValue(T value, float floor, float linear, float exponetial, int category)
        {
            Value = value;
            curve = new EvaluationCurve(floor, linear, exponetial);
            Category = category;
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
        [SerializeField] public int Category;

        public void LevelUp()
        {
            Level++;
            Value = (T)(ValueType)curve.EvaluateInt(Level);
        }

        public void LevelUp(int level)
        {
            Level = level;
            Value = (T)(object)curve.Evaluate(Level);
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

        public void OnBeforeSerialize()
        {
            if (Level == 0) curve.Floor = Convert.ToSingle(Value);

        }

        public void OnAfterDeserialize() { }
    }
}