using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using System;
using System.Reflection;
using Sirenix.Utilities;

public class Status : MonoBehaviour
{
    [SerializeField] protected string StatusName = "New Status";
    [SerializeField, ProgressBar(0, "MaxValue", r: 1, g: 1, b: 1, Height = 20, ColorGetter = "inspectorBarColor"), HideLabel, HorizontalGroup("colorBar")] protected float currentValue;
    [SerializeField, HideLabel, HorizontalGroup("colorBar", Width = 80)] protected Color inspectorBarColor = new Color(0.8f, 0.8f, 0.8f, 1);
    public float CurrentValue { get => currentValue; set => currentValue = value; }

    public LevelingValue<float> maxValue = new LevelingValue<float>(100f, 100, 1.2f, 1.1f);
    public float MaxValue { get => maxValue.Value; set => maxValue.Value = value; }

    [SerializeField, ToggleGroup("automaticRefill")] private bool automaticRefill;
    [SerializeField, ToggleGroup("automaticRefill")] private float refillRate = 3;
    [SerializeField, ToggleGroup("automaticRefill")] private float refillDelay = 3;

    [FoldoutGroup("Event")]
    public UnityEvent<Status> StatusUpdate = new UnityEvent<Status>();

    protected Coroutine ActiveAdjustment;

    protected bool showGizmos;

    public void OnEnable()
    {
        if (currentValue == 0) currentValue = maxValue.Value;
        if (automaticRefill && currentValue < maxValue.Value) ActiveAdjustment = StartCoroutine(AdjustOverTime(refillRate, refillDelay));
        StatusUpdate.Invoke(this);
    }

    public void AdjustStatus(float value)
    {
        currentValue = Mathf.Clamp(currentValue + value, 0, maxValue.Value);
        StatusUpdate.Invoke(this);

        if (ActiveAdjustment != null) StopCoroutine(ActiveAdjustment);
        if (automaticRefill) ActiveAdjustment = StartCoroutine(AdjustOverTime(refillRate, refillDelay));
    }

    public void SetMax(float value) {
        var currentRatio = currentValue / maxValue.Value;
        maxValue.Value = value;
        currentValue = maxValue.Value * currentRatio;
    }

    public IEnumerator AdjustOverTime(float AdjustRate)
    {
        while (currentValue < maxValue.Value)
        {
            currentValue = Mathf.Clamp(currentValue + AdjustRate * Time.deltaTime, 0, maxValue.Value);
            StatusUpdate?.Invoke(this);
            yield return null;
        }
    }

    public IEnumerator AdjustOverTime(float AdjustRate, float delay)
    {
        yield return new WaitForSeconds(delay);
        while (currentValue < maxValue.Value)
        {
            currentValue = Mathf.Clamp(currentValue + AdjustRate * Time.deltaTime, 0, maxValue.Value);
            StatusUpdate?.Invoke(this);
            yield return null;
        }
    }

    public void UpdateValueUpdaters() {
        SetMax(maxValue.GetPropertyValuefloat());
        AdjustStatus(0);
    }

    public static List<FieldInfo> GetAttributeValues<T>(object target) where T : Attribute
    {
        List<FieldInfo> values = new List<FieldInfo>();
        Type type = target.GetType();
        var fields = type.BaseType.GetFields( BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            T attributes = field.GetAttribute<T>();
            if (attributes != null)
            {
                values.Add(field);
            }
        }
        return values;
    }

    [ContextMenu("Set Color")]
    private void ChangeBarColor(Color newColor) {
        inspectorBarColor = newColor;
    }

    [ContextMenu("Show as Gizmos")]
    private void SwitchShowGizmos()
    {
        showGizmos = !showGizmos;
    }
    
    public void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Vector3 position;
            if (TryGetComponent(out Collider collider))
                position = collider.bounds.center + (collider.bounds.extents.magnitude * Vector3.up);
            else position = transform.position + Vector3.up;

            GUIContent content = new GUIContent(currentValue.ToString() + "/" + maxValue.ToString());
            GUIStyle labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, richText = true, fixedWidth = 100, fixedHeight = 20, fontSize = 7 };
        }
    }
}