using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign {
    public class ShotRayRender : MonoBehaviour
    {
        [SerializeField] private RangedAttack attack;
        [SerializeField] private LineRenderer line;

        [SerializeField] private float displayTime;

        // Start is called before the first frame update
        void Start()
        {
            if (!attack) attack = gameObject.GetComponent<RangedAttack>();
            if (!line) line = gameObject.GetComponent<LineRenderer>();
            line.enabled = false;
        }

        public void  ShootRay() {
            if (attack == null || line == null) return;

            line.SetPosition(0, transform.TransformPoint(attack.FirePoint));
            line.SetPosition(1, attack.FireDestination);

            StartCoroutine(ShowRay());

        }

        public IEnumerator ShowRay() {
            line.enabled = true;
            yield return null;
            yield return new WaitForSeconds(displayTime);

            line.enabled = false;
        }

    }
}
