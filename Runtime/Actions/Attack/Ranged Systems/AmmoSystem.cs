using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable, Toggle("Enabled")]
public class AmmoSystem
{
    public bool Enabled;
    [field: SerializeField] public int maxAmmo { get; set; } = 100;
    [field: SerializeField] public int currentAmmo { get; set; } = 50;
    [field: SerializeField] public bool useMagazine { get; set; }
    [field: SerializeField, ShowIf("useMagazine")] public int MagazineAmount { get; set; }
    [field: SerializeField, ShowIf("useMagazine")] public int magazineCapacity { get; set; } = 25;
    [field: SerializeField, ShowIf("useMagazine")] public int reloadTime { get; set; } = 1;

    public AmmoSystem() {
        Reload();
    }

    public void UseAmmo()
    {
        if (useMagazine)
        {
            MagazineAmount--;
            if (MagazineAmount <= 0) Reload();
        }
        else
            currentAmmo--;
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    }

    public void Reload() {
        if (currentAmmo >= magazineCapacity)
        {
            var reloadAmount = magazineCapacity - MagazineAmount;

            MagazineAmount += reloadAmount;
            currentAmmo -= reloadAmount;
        }
        else
        {
            MagazineAmount += currentAmmo;
            currentAmmo -= currentAmmo;
        }
    }

}
