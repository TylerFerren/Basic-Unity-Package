using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace Codesign
{
    public class StatManager : MonoBehaviour
    {
        [SerializeField] int UpgradePoints = 0;
        [InfoBox("Repeated Stat Category", InfoMessageType.Warning, "RepeatedCategoriesWarning")]
        [SerializeField] List<Stat> Stats;

        [ShowInInspector] public static string[] StatCategories = new string[6]{ "Health", "Stregth", "Speed", "Stanima", "Agility", "Dexterity" };
        public void OnEnable()
        {
            if (StatCategories == null) StatCategories = new string[0];
        }

        public void AddUpgradePoint(int addedPoints)
        {
            UpgradePoints += addedPoints;
        }

        public void UpgradeStat(Stat stat)
        {
            if (UpgradePoints <= 0) return;
            stat.StatUpgrade();
            UpgradePoints--;
        }

        public void UpgradeStat(string category)
        {

            var stat = Stats.Find(n => n.Category == Array.IndexOf(StatCategories, category));
            if (UpgradePoints <= 0) return;
            stat.StatUpgrade();
            UpgradePoints--;
        }

        private bool RepeatedCategoriesWarning()
        {
            return Stats.GroupBy(x => x.Category).Any(g => g.Count() > 1);
        }
    }

}