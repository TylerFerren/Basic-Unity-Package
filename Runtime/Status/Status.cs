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
        [SerializeField, ProgressBar(0, "MaxValue", r: 1, g: 1, b: 1, Height = 20, ColorGetter = "inspectorBarColor"), HideLabel, HorizontalGroup("colorBar")] protected float currentValue;
        [SerializeField, HideLabel, HorizontalGroup("colorBar", Width = 80)] protected Color inspectorBarColor = new Color(0.8f, 0.8f, 0.8f, 1);
        public Color InspectorBarColor { get{ return inspectorBarColor; } }
        public float CurrentValue { get => currentValue; set => currentValue = value; }

        public LevelingValue<float> maxValue = new LevelingValue<float>(100f, 100, 1.2f, 1.1f);
        public float MaxValue { get => maxValue; set => maxValue = value; }

        [SerializeField] private AutomaticUpdate _automaticUpdate;

        [FoldoutGroup("Event")]
        public UnityEvent<Status> StatusUpdate = new UnityEvent<Status>();
        [FoldoutGroup("Event")]
        public UnityEvent<float> StatusPercent = new UnityEvent<float>();

        protected Coroutine ActiveAdjustment;

        
        public void OnEnable()
        {
            if (currentValue == 0) currentValue = maxValue.Value;

            if (_automaticUpdate.Enabled) ActiveAdjustment = StartCoroutine(_automaticUpdate.Refill(this));

            StatusUpdate.Invoke(this);
            StatusPercent.Invoke(CurrentValue/MaxValue);
            maxValue.OnValueUpdate?.AddListener(() => AdjustStatus(maxValue.Value - maxValue.curve.EvaluateInt(maxValue.Level - 1)));
        }

        public void OnDisable()
        {
            maxValue.OnValueUpdate?.RemoveAllListeners();
        }

        public void AdjustStatus(float value)
        {
            currentValue = Mathf.Clamp(currentValue + value, 0, maxValue.Value);
            StatusUpdate.Invoke(this);
            StatusPercent.Invoke(CurrentValue / MaxValue);
            if (ActiveAdjustment != null) StopCoroutine(ActiveAdjustment);
            if (_automaticUpdate.Enabled) ActiveAdjustment = StartCoroutine(_automaticUpdate.Refill(this));
        }

        private void SetMax(float value)
        {
            var currentRatio = currentValue / maxValue.Value;
            maxValue.Value = value;
            currentValue = maxValue.Value * currentRatio;
        }

        public IEnumerator AdjustOverTime(float AdjustRate)
        {
            bool evaluation = AdjustRate > 0 ? currentValue < maxValue.Value : currentValue > 0;
            while (evaluation)
            {
                currentValue = Mathf.Clamp(currentValue + AdjustRate * Time.deltaTime, 0, maxValue.Value);
                StatusUpdate?.Invoke(this);
                StatusPercent?.Invoke(CurrentValue / MaxValue);
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

        public void Refill(float value) {
            currentValue = Mathf.Clamp(currentValue + value, 0, maxValue.Value);
            StatusUpdate.Invoke(this);
            StatusPercent.Invoke(CurrentValue / MaxValue);
        }


    #if UNITY_EDITOR

        [SerializeField, HideInInspector] protected bool showGizmos;
        [SerializeField, ShowIf("showGizmos"), FoldoutGroup("Gizmos Settings")] private Vector3 gizmosOffset = new Vector3();
        [SerializeField, ShowIf("showGizmos"), FoldoutGroup("Gizmos Settings"), Range(5, 20)] private int gizmosSize = 7;

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

                GUIContent content = new GUIContent(Mathf.Round(currentValue).ToString() + "/" + maxValue.Value.ToString() + " HP");
                GUIStyle labelStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, richText = false, fixedWidth = 120, fixedHeight = 30, fontSize = gizmosSize, fontStyle = FontStyle.Bold};
                labelStyle.normal.textColor = inspectorBarColor;
                Handles.Label(position, content, labelStyle);
            }
        }
    #endif
    }
}