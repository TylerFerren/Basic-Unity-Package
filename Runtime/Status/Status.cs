using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;
using System.ComponentModel;

public class Status : MonoBehaviour
{
    [SerializeField, ProgressBar(0, "maxValue", r: 1, g: 1, b: 1, Height = 20, ColorGetter = "InspectorBarColor"), HideLabel, Header("Current Value")]
    protected float currentValue;
    public float CurrentValue { get => currentValue; set => currentValue = value; }
    [SerializeField, ]private Color inspectorBarColor = new Color(0.8f, 0.8f, 0.8f, 1);

    [SerializeField]
    protected float maxValue = 100f;
    public float MaxValue { get => maxValue; set => maxValue = value; }


    [SerializeField, ToggleGroup("automaticRefill")] private bool automaticRefill;
    [SerializeField, ToggleGroup("automaticRefill")] private float refillRate = 3;
    [SerializeField, ToggleGroup("automaticRefill")] private float refillDelay = 3;


    [FoldoutGroup("Event")]
    public UnityEvent<Status> StatusUpdate = new UnityEvent<Status>();

    protected Color InspectorBarColor = new Color(0.8f, 0.8f, 0.8f, 1);
    protected Coroutine ActiveAdjustment;

    [SerializeField, HideInInspector]
    private bool showGizmos;

    public void OnEnable()
    {
        if (currentValue == 0) currentValue = maxValue;
        if (automaticRefill && currentValue < maxValue) ActiveAdjustment = StartCoroutine(AdjustOverTime(refillRate, refillDelay));
        StatusUpdate.Invoke(this);
    }

    private void OnValidate()
    {
        InspectorBarColor = inspectorBarColor;
    }

    public void AdjustStatus(float value)
    {
        currentValue = Mathf.Clamp(currentValue + value, 0, maxValue);
        StatusUpdate.Invoke(this);

        if (ActiveAdjustment != null) StopCoroutine(ActiveAdjustment);
        if (automaticRefill) ActiveAdjustment = StartCoroutine(AdjustOverTime(refillRate, refillDelay));

    }

    public IEnumerator AdjustOverTime(float AdjustRate)
    {
        while (currentValue < maxValue)
        {
            currentValue = Mathf.Clamp(currentValue + AdjustRate * Time.deltaTime, 0, maxValue);
            StatusUpdate?.Invoke(this);
            yield return null;
        }
    }

    public IEnumerator AdjustOverTime(float AdjustRate, float delay)
    {
        yield return new WaitForSeconds(delay);
        while (currentValue < maxValue)
        {
            currentValue = Mathf.Clamp(currentValue + AdjustRate * Time.deltaTime, 0, maxValue);
            StatusUpdate?.Invoke(this);
            yield return null;
        }
    }

    [ContextMenu("Show as Gizmos")]
    private void ShowGizmos()
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

            //Handles.Label(position, content, labelStyle);
        }
    }
}