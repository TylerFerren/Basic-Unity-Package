using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Codesign
{
    [Serializable]
    public class Stat
    {
        [field: SerializeField, CustomValueDrawer("NameSelectorDrawer")] public int Category { get; set; }
        [field: SerializeField] public int CurrentValue { get; set; } = 0;
        [field: SerializeField] public int MaxValue { get; set; } = 10;

        [FoldoutGroup("Event")]
        public UnityEvent<int> OnStatUpdate;


        [Button]
        public void StatUpgrade()
        {
            if (CurrentValue == MaxValue) return;
            CurrentValue++;
            OnStatUpdate?.Invoke(CurrentValue);
        }

        public int NameSelectorDrawer(int index, GUIContent label)
        {
            return EditorGUILayout.Popup(label, index, StatManager.StatCategories);
        }

    }
}
