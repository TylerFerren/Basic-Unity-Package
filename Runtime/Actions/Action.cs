using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Codesign {
    public abstract class Action : MonoBehaviour
    {
        [Title("Action Settings")]
        public bool IsActive { get; set; } = false;

        
        public Coroutine ActiveAutomaticCycle;
        [ReadOnly] public bool AutomaticIsActive = false;

        [SerializeField] protected CooldownSystem cooldown = new CooldownSystem();
        [SerializeField] protected ChargeSystem charge = new ChargeSystem();

        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected bool useStatus;
        [SerializeField, ToggleGroup("useStatus", "Use Status")] protected Status status;

        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnTrigger;
        [SerializeField, FoldoutGroup("Events")] protected UnityEvent OnRelease;

        public Coroutine activeAction;

        public IEnumerator AutomaticCycle() {
            while (true) {
                while (AutomaticIsActive) {
                    yield return activeAction = StartCoroutine(Trigger());
                }
                if(activeAction != null) yield return StartCoroutine(Finish());
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

        public void TriggerAction() => StartCoroutine(FullTrigger());

        public virtual IEnumerator FullTrigger() {
            yield return StartCoroutine(Trigger());
            yield return StartCoroutine(Release());
            yield return StartCoroutine(Finish());
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