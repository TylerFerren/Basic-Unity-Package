using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using System.Linq;

namespace Codesign {

    public class ActionManager : MonoBehaviour
    {
        public List<ActionInstance> Actions = new List<ActionInstance>();

        public void OnValidate()
        {
            var actions = GetComponents<Action>();
            foreach(Action instance in actions) {
                if (Actions.Where(n => n.action == instance).Count() == 0) {
                    Actions.Add(new ActionInstance() { action = instance });    
                }
            }
        }

        public void AddAction(Action action, ActionTriggerType triggerType, InputActionReference input) {
            ActionInstance newAction = new ActionInstance()
            {
                action = action,
                triggerType = triggerType,
                inputRef = input
            };
            newAction.EnableInstance();
            Actions.Add(newAction);
        }

        private void OnEnable()
        {
            foreach (ActionInstance instance in Actions)
            {
                instance.EnableInstance();
                print(instance.action.name);
            }
        }

        private void OnDisable()
        {
            foreach (ActionInstance instance in Actions)
            {
                instance.DisableInstance();
            }
        }
    }

    public enum ActionTriggerType { UserInput, Automatic, ExternalTrigger }

    [Serializable]
    public struct ActionInstance
    {
        public Action action;
        public ActionTriggerType triggerType;
        [ShowIf("triggerType", ActionTriggerType.UserInput)] public InputActionReference inputRef;

        public void EnableInstance()
        {
            if (!action && !inputRef && triggerType == ActionTriggerType.UserInput) return;
            inputRef.action.Enable();
            inputRef.action.performed += action.InputMethod;
            inputRef.action.canceled += action.InputMethod;

            if (triggerType == ActionTriggerType.Automatic)
            {
                action.ActiveAutomaticCycle = action.StartCoroutine(action.AutomaticCycle());
            }
        }

        public void DisableInstance()
        {
            if (!action && !inputRef && triggerType == ActionTriggerType.UserInput) return;
            inputRef.action.Disable();
            inputRef.action.performed -= action.InputMethod;
            inputRef.action.canceled -= action.InputMethod;

            if (action.ActiveAutomaticCycle != null) action.StopCoroutine(action.ActiveAutomaticCycle);

        }

        public void SetInstanceAction(Action _action) {
            action = _action;
        }

    }
}