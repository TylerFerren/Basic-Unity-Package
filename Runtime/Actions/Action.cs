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

        [Title("Action Settings")]
        [field: SerializeField] public bool IsActive { get; set; } = false;
        public List<Collider> ActorColliers;

        [SerializeField] protected ActionTriggerType triggerType = ActionTriggerType.UserInput;
        [SerializeField, ShowIf("triggerType", ActionTriggerType.UserInput)] protected InputActionReference inputRef;
        public Coroutine ActiveAutomaticCycle;
        public bool AutomaticIsActive = true;

        [SerializeField] protected CooldownSystem cooldown = new CooldownSystem();
        [SerializeField] protected ChargeSystem charge = new ChargeSystem();

        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected bool useStatus;
        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected Status status;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnTrigger;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnRelease;

        public Coroutine activeAction;

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
                    yield return activeAction = StartCoroutine(Trigger());
                }
                yield return StartCoroutine(Release());
                yield return new WaitUntil(() => AutomaticIsActive && activeAction == null);
            }
        }


        public void InputMethod(InputAction.CallbackContext context) {
            
            if (context.performed && activeAction == null) {
                activeAction = StartCoroutine(Trigger());
            }
            else if(context.canceled && IsActive) {
                StartCoroutine(Release());
            }
        }

        public virtual IEnumerator Trigger()
        {
            if (cooldown.Enabled && cooldown.ActiveCooldown != null) yield break;

            if (charge.Enabled ) {
                if (charge.charageOnce && !IsActive || !charge.charageOnce) {
                    IsActive = true;
                    yield return StartCoroutine(charge.Charge());
                }
                
                if (!IsActive) yield break;
            }
            IsActive = true;
        }

        public virtual IEnumerator Release()
        {
            IsActive = false;
            StartCoroutine(Finish());
            yield return null;
        }

        public virtual IEnumerator Finish()
        {
            yield return activeAction;
            activeAction = null;
            IsActive = false;
            yield return null;
        }

    }

}