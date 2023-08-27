using UnityEngine;


namespace Codesign
{
    public class Energy : Status
    {
        [SerializeField, Range(0,1)] private float EnergyStealRatio = 0.5f;

        public void Reset()
        {
            inspectorBarColor = new Color(0.4f, 0.5f, 0.9f, 1);
            currentValue = MaxValue / 2;
        }

        public void GetEnergyFrom(Component component) {
            print(component);
            if (component.TryGetComponent(out Energy energy)) {
                AdjustStatus(energy.currentValue * EnergyStealRatio);
            }
        }
    }
}
