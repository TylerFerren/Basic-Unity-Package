using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EvaluationCurve 
{
    
    public float Floor;
    public float LinearGain;
    public float ExponetialGain;
    [SerializeField, Range(1, 100)] private int TestInput = 1;
    [SerializeField, ReadOnly] private int testOutput;

    public EvaluationCurve() { }
    public EvaluationCurve(float floor, float linearGain, float exponetialGain) {
        Floor = floor;
        LinearGain = linearGain;
        ExponetialGain = exponetialGain;
    }

    public float Evaluate(float x) {
        var y = Mathf.Pow(x * LinearGain -1, ExponetialGain) + Floor;
            return y;
    }

    public int EvaluateInt(int x) {
        int y = Mathf.RoundToInt(Mathf.Pow(x * LinearGain - 1, ExponetialGain) + Floor);
        return y;
    }

    public int TestEvaluate() {
        return EvaluateInt(TestInput);
    }
}
