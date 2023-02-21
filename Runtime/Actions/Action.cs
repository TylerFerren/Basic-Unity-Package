using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

namespace Codesign {
    public abstract class Action : MonoBehaviour
    {
        public enum ActionTriggerType {UserInput, Automatic}
        public bool IsActive { get; set; } = false;
        public ActionManager Actor { get; set; }
        [Title("Action Settings")]
        [SerializeField] protected ActionTriggerType triggerType = ActionTriggerType.UserInput;
        [SerializeField, ShowIf("triggerType", ActionTriggerType.UserInput)] protected InputActionReference inputRef;
        protected Coroutine ActiveAutomaticCycle;
        protected bool AutomaticIsActive = true;

        [SerializeField, ToggleGroup("useCooldown", "Use Cooldown")] protected bool useCooldown;
        [SerializeField, ToggleGroup("useCooldown")] protected LevelingValue<float> cooldownTime = new LevelingValue<float>(3, 3, 0.66f, 1);
        [SerializeField, ProgressBar(0, "cooldownTime", r: 1, g: 1, b: 1, Height = 20), ToggleGroup("useCooldown")] protected float CurrentTimer;
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
            if(ActiveAutomaticCycle != null) StopCoroutine(ActiveAutomaticCycle);
        }

        public void Start()
        {
            if (triggerType == ActionTriggerType.Automatic) {
                ActiveAutomaticCycle = StartCoroutine(AutomaticCycle());
            }
        }

        public IEnumerator AutomaticCycle() {
            while (triggerType == ActionTriggerType.Automatic) {
                while (AutomaticIsActive) {
                    Trigger(new InputAction.CallbackContext());
                    yield return new WaitUntil(() => !IsActive);
                    yield return new WaitUntil(() => ActiveCooldown == null);
                }
                yield return new WaitUntil(() => AutomaticIsActive);
            }
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
            CurrentTimer = cooldownTime;
            while (CurrentTimer > 0)
            {
                CurrentTimer -= Time.deltaTime;
                yield return null;
            }
            CurrentTimer = 0;
            ActiveCooldown = null;
        }

        public IEnumerator Cooldown(float cooldownTime)
        {
            CurrentTimer = cooldownTime;
            while (CurrentTimer > 0)
            {
                CurrentTimer -= Time.deltaTime;
                yield return null;
            }
            CurrentTimer = 0;
            ActiveCooldown = null;
        }


    }

}