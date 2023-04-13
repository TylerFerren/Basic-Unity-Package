using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign {
    public class ShotRayRender : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;

        [SerializeField] private float displayTime;

        // Start is called before the first frame update
        void Start()
        {
            if (!line) line = gameObject.GetComponent<LineRenderer>();
            line.enabled = false;
        }

        public void  ShootRay(Vector3 start, Vector3 end) {
            if (line == null) return;

            line.SetPosition(0, start);
            line.SetPosition(1, end);

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
