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

namespace Codesign
{
    public class Status : MonoBehaviour
    {
        [SerializeField] protected string StatusName = "New Status";
        [SerializeField, ProgressBar(0, "MaxValue", r: 1, g: 1, b: 1, Height = 20, ColorGetter = "inspectorBarColor"), HideLabel, HorizontalGroup("colorBar")] protected float currentValue;
        [SerializeField, HideLabel, HorizontalGroup("colorBar", Width = 80)] protected Color inspectorBarColor = new Color(0.8f, 0.8f, 0.8f, 1);
        public float CurrentValue { get => currentValue; set => currentValue = value; }

        public LevelingValue<float> maxValue = new LevelingValue<float>(100f, 100, 1.2f, 1.1f);
        public float MaxValue { get => maxValue; set => maxValue = value; }

        [SerializeField, ToggleGroup("automaticRefill")] private bool automaticRefill;
        [SerializeField, ToggleGroup("automaticRefill")] private float refillRate = 3;
        [SerializeField, ToggleGroup("automaticRefill")] private float refillDelay = 3;

        [FoldoutGroup("Event")]
        public UnityEvent<Status> StatusUpdate = new UnityEvent<Status>();

        protected Coroutine ActiveAdjustment;

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

        private void SetMax(float value)
        {
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

        public void UpdateMaxValue()
        {
            maxValue.LevelUp();
            SetMax(maxValue.Value);
            AdjustStatus(0);
        }

        public static List<FieldInfo> GetAttributeValues<T>(object target) where T : Attribute
        {
            List<FieldInfo> values = new List<FieldInfo>();
            Type type = target.GetType();
            var fields = type.BaseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
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


#if UNITY_EDITOR

    [SerializeField, HideInInspector] protected bool showGizmos;
    [SerializeField, ShowIf("showGizmos")] private Vector3 gizmosOffset = new Vector3();

    [ContextMenu("Show as Gizmos")]
    private void SwitchShowGizmos()
    {
        showGizmos = !showGizmos;
    }

    public void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Vector3 position = transform.position + gizmosOffset;

            GUIContent content = new GUIContent(currentValue.ToString("D") + "/" + maxValue.Value.ToString("D") + " HP");
            GUIStyle labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, richText = false, fixedWidth = 120, fixedHeight = 30, fontSize = 12, fontStyle = FontStyle.Bold};
            labelStyle.normal.textColor = inspectorBarColor;
            Handles.Label(position, content, labelStyle);
        }
    }
#endif
    }
}