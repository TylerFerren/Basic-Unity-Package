using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Codesign { 
    public abstract class Action : MonoBehaviour
    {
        public bool IsActive { get; set; } = false;
        public ActionManager Actor { get; set; }
        [Title("Action Settings")]
        [SerializeField] protected InputActionReference inputRef;
        [SerializeField] protected bool useCooldown;
        [SerializeField] protected LevelingValue<float> cooldownTime = new LevelingValue<float>(3, 3, 0.66f, 1);
        public float CurrentTimer { get; set; }
        protected Coroutine ActiveCooldown;

        [SerializeField, ToggleGroup("useStatus")] protected bool useStatus;
        [SerializeField, ToggleGroup("useStatus")] protected Status status;

        [SerializeField, ToggleGroup("TriggerAnimation")] protected bool TriggerAnimation;
        [SerializeField, ToggleGroup("TriggerAnimation")] protected Animator animator;
        [SerializeField, ToggleGroup("TriggerAnimation")] protected string animatorParameter;

        public void OnEnable()
        {
            if (!inputRef) return;
            inputRef.action.Enable();
            inputRef.action.performed += Trigger;
            inputRef.action.canceled += Release;
        }

        public void OnDisable()
        {
            if (!inputRef) return;
            inputRef.action.Disable();
            inputRef.action.performed -= Trigger;
            inputRef.action.canceled -= Release;
        }

        public virtual void Trigger(InputAction.CallbackContext context)
        {
            IsActive = true;
        }

        public virtual void Release(InputAction.CallbackContext context)
        {
            IsActive = false;
        }

        public IEnumerator Cooldown()
        {
            CurrentTimer = 0;
            while (CurrentTimer < cooldownTime)
            {
                CurrentTimer += Time.deltaTime;
                yield return null;
            }
            ActiveCooldown = null;
        }

        public IEnumerator Cooldown(float cooldownTime)
        {
            CurrentTimer = 0;
            while (CurrentTimer < cooldownTime)
            {
                CurrentTimer += Time.deltaTime;
                yield return null;
            }
            ActiveCooldown = null;
        }
    }

}