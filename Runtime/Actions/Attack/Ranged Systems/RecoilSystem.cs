using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public class RecoilSystem : ActionSystem
    {
        [field: SerializeField] public float climb { get; private set; }
        
        public CinemachineVirtualCamera camera;
        private CinemachinePOV POV;

        public void Recoil() {
            POV = camera.GetCinemachineComponent<CinemachinePOV>();
            Vector3 RecoilVector = new Vector3(0, climb, 0);
            if(POV) POV.m_VerticalAxis.Value -= climb;
        }
    }
}