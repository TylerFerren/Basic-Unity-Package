using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Codesign
{

    public class AimSystem : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private float AimFieldOfView = 40;
        [SerializeField, Range(0, 5)] private float AimTransitionSpeed = 0.2f;

        [SerializeField, Range(0, 2)] private float AimSpeedRatio = 1;

        public bool Aiming { get; private set; }

        public UnityEvent<bool> IsAiming;

        private float FOVBase;

        private CinemachinePOV cinemachinePOV;
        private float horizontalSpeedBase;
        private float verticalSpeedBase;

        public void OnAim(InputAction.CallbackContext context)
        {
            Aiming = context.performed;
            IsAiming?.Invoke(Aiming);
        }

        public void Start()
        {
            if (virtualCamera)
            {
                FOVBase = virtualCamera.m_Lens.FieldOfView;
                cinemachinePOV = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
                horizontalSpeedBase = cinemachinePOV.m_HorizontalAxis.m_MaxSpeed;
                verticalSpeedBase = cinemachinePOV.m_VerticalAxis.m_MaxSpeed;
            }   
        }

        public void FixedUpdate()
        {
            if (virtualCamera == null) return;

            if (Aiming)
            {
                virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, AimFieldOfView, Time.fixedDeltaTime / AimTransitionSpeed);
                cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = horizontalSpeedBase * AimSpeedRatio;
                cinemachinePOV.m_VerticalAxis.m_MaxSpeed = verticalSpeedBase * AimSpeedRatio;
            }
            else
            {
                virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, FOVBase, Time.fixedDeltaTime / AimTransitionSpeed);
                cinemachinePOV.m_HorizontalAxis.m_MaxSpeed = horizontalSpeedBase;
                cinemachinePOV.m_VerticalAxis.m_MaxSpeed = verticalSpeedBase;
            }
        }

    }
}
