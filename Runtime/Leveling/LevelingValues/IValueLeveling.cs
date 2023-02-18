using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IValueLeveling
{
    public static void LevelUpValue<T>(LevelingValue<T> levelingValue)
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
