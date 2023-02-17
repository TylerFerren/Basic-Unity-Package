using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[Serializable]
public class Stat
{
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField, CustomValueDrawer("NameSelectorDrawer")] public int Category { get; set; }
    [field: SerializeField] public int CurrentValue { get; set; } = 1;
    [field: SerializeField] public int MaxValue { get; set; } = 10;

    [FoldoutGroup("Event")]
    public UnityEvent OnStatUpdate;

    [Button]
    public void StatUpgrade() {
        if (CurrentValue == MaxValue) return;
        CurrentValue++; 
        OnStatUpdate?.Invoke();

    }

    private int NameSelectorDrawer(int index, GUIContent label)
    {
        return EditorGUILayout.Popup( label, index, StatManager.StatCategories.ToArray()) ;
    }

}
