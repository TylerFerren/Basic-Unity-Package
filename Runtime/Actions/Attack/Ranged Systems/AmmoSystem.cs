using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public class AmmoSystem : ActionSystem
    {
        [field: SerializeField] public int maxAmmo { get; set; } = 100;
        [field: SerializeField] public int currentAmmo { get; set; } = 50;
        [field: SerializeField] public bool useMagazine { get; set; }
        [field: SerializeField, ShowIf("useMagazine")] public int MagazineAmount { get; set; }
        [field: SerializeField, ShowIf("useMagazine")] public int magazineCapacity { get; set; } = 25;
        [field: SerializeField, ShowIf("useMagazine")] public int reloadTime { get; set; } = 1;

        public AmmoSystem()
        {
            Reload();
        }

        public void UseAmmo()
        {
            if (useMagazine)
            {
                MagazineAmount--;
                if (MagazineAmount <= 0) Task.Run(Reload);
            }
            else
                currentAmmo--;
        }

        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        }

        public async void Reload()
        {
            await Task.Delay(reloadTime * 1000);
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

}