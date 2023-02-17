using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelingValue<T>
{
    public LevelingValue(T value) {
        Value = value;
    }

    public LevelingValue(T value, float floor, float linear, float exponetial)
    {
        Value = value;
        curve = new EvaluationCurve(floor, linear, exponetial);
    }

    public T Value;
    public int Level = 0;
    public EvaluationCurve curve = new EvaluationCurve();

    public int GetPropertyValueInt()
    {
        Level++;
        return curve.EvaluateInt(Level);
    }

    public float GetPropertyValuefloat()
    {
        Level++;
        return curve.Evaluate(Level);
    }
}
