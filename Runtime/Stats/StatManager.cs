using System.Linq;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    [SerializeField] public static List<string> StatCategories = new List<string>() {"Health", "Stregth", "Speed", "Stanima", "Agility", "Dexterity"};
    [SerializeField] int UpgradePoints = 0;
    [InfoBox("Repeated Stat Category", InfoMessageType.Warning, "RepeatedCategories")]
    [SerializeField] List<Stat> Stats;

    public void OnEnable()
    {
        if(StatCategories == null) StatCategories = new List<string>();
    }

    public void AddUpgradePoint(int addedPoints) {
        UpgradePoints += addedPoints;
    }

    public void UpgradeStat(Stat stat) {
        if (UpgradePoints <= 0) return;
        stat.StatUpgrade();
        UpgradePoints--;
    }

    public void UpgradeStat(string statName) {
        var stat = Stats.Find(n => n.Category == StatCategories.IndexOf(statName));
        if (UpgradePoints <= 0) return;
        stat.StatUpgrade();
        UpgradePoints--;
    }

    private bool RepeatedCategories()
    {
        return Stats.GroupBy(x => x.Category).Any(g => g.Count() > 1);
    }
}
