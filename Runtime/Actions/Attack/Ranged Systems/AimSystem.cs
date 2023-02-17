using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AimSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float AimFieldOfView = 40;
    [SerializeField, Range(0, 5)] private float AimTransitionSpeed = 0.2f;
    public bool Aiming { get; private set; }

    public UnityEvent<bool> IsAiming;

    private float FOVBase;
    
    public void OnAim(InputAction.CallbackContext context) {
        Aiming = context.performed;
        IsAiming?.Invoke(Aiming);
    }

    public void Start()
    {
        if(virtualCamera)
            FOVBase = virtualCamera.m_Lens.FieldOfView;
    }

    public void FixedUpdate()
    {
        if (virtualCamera == null) return;

        if (Aiming) virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, AimFieldOfView, Time.fixedDeltaTime / AimTransitionSpeed);
        else virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, FOVBase, Time.fixedDeltaTime / AimTransitionSpeed);
    }

}
