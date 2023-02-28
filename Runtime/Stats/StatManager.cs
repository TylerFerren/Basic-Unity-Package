using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Reflection;

namespace Codesign
{
    public class StatManager : MonoBehaviour
    {
        [SerializeField] int upgradePoints = 0;
        public int UpgradePoints { get { return upgradePoints; } set { upgradePoints = value; } }
        [InfoBox("Repeated Stat Category", InfoMessageType.Warning, "RepeatedCategoriesWarning")]
        [SerializeField] public List<Stat> Stats;
        [Space]
        [ShowInInspector] public static string[] StatCategories = new string[7]{"None", "Health", "Stregth", "Speed", "Stanima", "Agility", "Dexterity" };

        [SerializeField, HideInInspector] public LevelingValueRefrencs levelingValues = new LevelingValueRefrencs();

        public void FindLevelingValues()
        {
            levelingValues = new LevelingValueRefrencs();
            foreach (Stat stat in Stats) {
                stat.levelingValues = new List<LevelingValue<float>>();
            }
            Component[] components;

            if(transform.root != transform)
                components = transform.parent.GetComponentsInChildren(typeof(MonoBehaviour), true);
            else
                components = transform.GetComponentsInChildren(typeof(MonoBehaviour), true);

            foreach (var component in components) {
                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(LevelingValue<float>))
                    {
                        var levelingValue = field.GetValue(component) as LevelingValue<float>;
                        var stat = Stats.Find(n => n.Category == levelingValue.Category);
                        if(stat != null && !stat.levelingValues.Contains(levelingValue)) stat.levelingValues.Add(levelingValue);
                    }

                    #region subFields
                    //FieldInfo[] subfields = field.FieldType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    //foreach (FieldInfo subfield in subfields) {
                    //    if (subfield.FieldType == typeof(LevelingValue<float>))
                    //    {
                    //        Debug.Log(field.GetValue(component) as LevelingValue<float>);

                    //        var levelingValue = subfield.GetValue(field.GetValue(component)) as LevelingValue<float>;
                    //        levelingValue.statManager = this;
                    //    }
                    //}
                    #endregion
                }
            }
        }

        private void OnValidate()
        {
            foreach (Stat stat in Stats) {
                stat.manager = this;
            }
            FindLevelingValues();

        }

        public void OnEnable()
        {
            StatCategories ??= new string[0];
        }

        public void AddUpgradePoint(int addedPoints)
        {
            upgradePoints += addedPoints;
        }

        public void UpgradeStat(Stat stat)
        {
            if (upgradePoints <= 0) return;
            stat.StatUpgrade();
        }

        public void UpgradeStat(string category)
        {
            var stat = Stats.Find(n => n.Category == Array.IndexOf(StatCategories, category));
            if (upgradePoints <= 0) return;
            stat.StatUpgrade();
            upgradePoints--;
        }

        public bool RepeatedCategoriesWarning()
        {
            return Stats.GroupBy(x => x.Category).Any(g => g.Count() > 1);
        }
    }

    [Serializable]
    public class LevelingValueRefrencs : Dictionary<Stat, LevelingValue<float>> { }
}