using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

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

        [SerializeField] protected CooldownSystem cooldown = new CooldownSystem();
        [SerializeField] protected ChargeSystem charge = new ChargeSystem();

        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected bool useStatus;
        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected Status status;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnTrigger;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnRelease;


        public virtual void OnEnable()
        {
            if (triggerType == ActionTriggerType.UserInput)
            {
                if(!inputRef) return;
                inputRef.action.Enable();
                inputRef.action.performed += InputMethod;
                inputRef.action.canceled += InputMethod;
            }
            else if (triggerType == ActionTriggerType.Automatic)
            {
                ActiveAutomaticCycle = StartCoroutine(AutomaticCycle());
            }
        }

        public virtual void OnDisable()
        {
            if (triggerType == ActionTriggerType.UserInput)
            {
                if (!inputRef) return;
                inputRef.action.Disable();
                inputRef.action.performed -= InputMethod;
                inputRef.action.canceled -= InputMethod;
            }
            else if (ActiveAutomaticCycle != null) StopCoroutine(ActiveAutomaticCycle);
        }

        public IEnumerator AutomaticCycle() {
            while (triggerType == ActionTriggerType.Automatic) {
                while (AutomaticIsActive) {
                    yield return StartCoroutine(Trigger());
                }
                yield return new WaitUntil(() => AutomaticIsActive);
            }
        }

        public void InputMethod(InputAction.CallbackContext context) {
            if (context.performed) StartCoroutine(Trigger());
            if (context.canceled) StartCoroutine(Release());
        }

        public virtual IEnumerator Trigger()
        {
            IsActive = true;
            if (charge.Enabled && charge.charageOnce) {
                yield return StartCoroutine(charge.Charge());
            }

            if (cooldown.Enabled && cooldown.ActiveCooldown != null) {
                IsActive = false;
                yield break;
            }

        }

        public virtual IEnumerator Release()
        {
            IsActive = false;
            yield return null;
        }

    }

}