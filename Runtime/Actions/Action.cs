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
        public bool IsUnavailable { get; set; } = false;
        public bool IsActive { get; set; } = false;

        
        public Coroutine ActiveAutomaticCycle;
        [HideInInspector] public bool AutomaticIsActive = false;

        [ FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent OnTrigger;
        [ FoldoutGroup("Events"), PropertyOrder(99)] public UnityEvent OnRelease;

        public Coroutine activeAction;

        public virtual IEnumerator Trigger()
        {
            if(IsUnavailable) yield break;
            IsActive = true;
            yield return null;
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

        public void TriggerAction() => StartCoroutine(FullActionCycle());

        public virtual IEnumerator FullActionCycle() {
            yield return StartCoroutine(Trigger());
            yield return StartCoroutine(Release());
            yield return StartCoroutine(Finish());
        }

        

    }

}