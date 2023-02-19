using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codesign {

    public class ActionManager : MonoBehaviour
    {
        public List<ActionInstance> actions = new List<ActionInstance>();

        private void OnEnable()
        {
            foreach (ActionInstance instance in actions)
            {
                instance.EnableInstance();
            }
        }

        private void OnDisable()
        {
            foreach (ActionInstance instance in actions)
            {
                instance.DisableInstance();
            }
        }
    }

    [Serializable]
    public struct ActionInstance
    {
        public Action action;
        public InputActionReference inputRef;

        public void EnableInstance()
        {
            if (!action && !inputRef) return;
            inputRef.action.Enable();
            inputRef.action.performed += action.Trigger;
            inputRef.action.canceled += action.Release;
        }

        public void DisableInstance()
        {
            if (!action && !inputRef) return;
            inputRef.action.Disable();
            inputRef.action.performed -= action.Trigger;
            inputRef.action.canceled -= action.Release;
        }

    }
}