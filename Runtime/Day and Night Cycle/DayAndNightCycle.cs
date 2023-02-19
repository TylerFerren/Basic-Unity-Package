using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign
{
    public class DayAndNightCycle : MonoBehaviour
    {
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;

        [SerializeField] private float cycleLength;
        [SerializeField] private float lightYRotation;

        private float currentCycleTime;

        private void OnValidate()
        {
            if (sunLight) sunLight.transform.eulerAngles.Set(sunLight.transform.eulerAngles.x, lightYRotation, sunLight.transform.eulerAngles.z);
            if (moonLight) moonLight.transform.eulerAngles.Set(moonLight.transform.eulerAngles.x, lightYRotation, moonLight.transform.eulerAngles.z);
            CalcLightPosition(currentCycleTime);
        }

        private void Awake()
        {
            if (!sunLight || !moonLight) enabled = false;
        }

        private void FixedUpdate()
        {
            if (currentCycleTime >= cycleLength) currentCycleTime = 0;

            CalcLightPosition(currentCycleTime);

            currentCycleTime += Time.fixedDeltaTime;

            //if (sunLight.transform.eulerAngles.x > 180) sunLight.enabled = false;
            //else sunLight.enabled = true;

            //if (moonLight.transform.eulerAngles.z < 360) moonLight.enabled = false;
            //else moonLight.enabled = true;
        }

        private void CalcLightPosition(float currentTime)
        {
            sunLight.transform.eulerAngles = Vector3.Lerp(new Vector3(0, lightYRotation, sunLight.transform.eulerAngles.z), new Vector3(360, lightYRotation, sunLight.transform.eulerAngles.z), currentTime / cycleLength);
            moonLight.transform.eulerAngles = Vector3.Lerp(new Vector3(180, lightYRotation, moonLight.transform.eulerAngles.z), new Vector3(540, lightYRotation, moonLight.transform.eulerAngles.z), currentTime / cycleLength);
        }
    }

}