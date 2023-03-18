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

        private void OnEnable()
        {
            foreach (ActionInstance instance in Actions)
            {
                instance.EnableInstance();
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
        [SerializeField] private ActionTriggerType triggerType;
        [SerializeField, ShowIf("triggerType", ActionTriggerType.UserInput)] private InputActionReference inputRef;

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


    }
}