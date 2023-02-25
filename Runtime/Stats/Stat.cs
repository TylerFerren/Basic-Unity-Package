using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using System.Linq;

namespace Codesign
{
    [Serializable]
    public class Stat
    {
        public Stat() { }
        public Stat(StatManager statManager) { manager = statManager; }
        public StatManager manager { get; set; }

        [field: SerializeField, CustomValueDrawer("NameSelectorDrawer")] public int Category { get; set; } = 1;
        [field: SerializeField] public int CurrentValue { get; set; } = 0;
        [field: SerializeField] public int MaxValue { get; set; } = 10;

        [ShowInInspector] public List<LevelingValue<float>> levelingValues = new List<LevelingValue<float>>();


        [FoldoutGroup("Event")]
        public UnityEvent<int> OnStatUpdate;

        [Button]
        public void StatUpgrade()
        {
            if (manager.UpgradePoints <= 0) return;
            if (CurrentValue == MaxValue) return;
            CurrentValue++;
            manager.UpgradePoints--;
            foreach (LevelingValue<float> value in levelingValues) {
                value.LevelUp(CurrentValue);
            }

            OnStatUpdate?.Invoke(CurrentValue);
        }

        public int NameSelectorDrawer(int index, GUIContent label)
        {
            return EditorGUILayout.Popup(label, index, StatManager.StatCategories);
        }
    }
}
