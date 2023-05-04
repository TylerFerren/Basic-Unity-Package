using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Codesign
{
    [Serializable]
    public class Stat
    {
        public Stat() { }
        public Stat(StatManager statManager) { manager = statManager; }
        public StatManager manager { get; set; }

        [SerializeField, CustomValueDrawer("NameSelectorDrawer")] private int category = 1;
        public int Category { get { return category; } set { category = value; } }
        [SerializeField] private int currentValue = 0;
        public int CurrentValue { get { return currentValue; } set { currentValue = value; } }
        [SerializeField] private int maxValue = 10;
        public int MaxValue { get { return maxValue; } set { maxValue = value; } }

        [ShowInInspector] public List<LevelingValue<float>> levelingValues = new List<LevelingValue<float>>();

        [FoldoutGroup("Event")]
        public UnityEvent<int> OnStatUpdate;

        public void StatUpgrade()
        {
            if (CurrentValue == MaxValue) return;
            CurrentValue++;
            manager.UpgradePoints--;
            foreach (LevelingValue<float> value in levelingValues)
            {
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
