using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign {
    public class ActionSequence : Action
    {
        [SerializeField] private List<ActionSequenceItem> actions;
        private int actionIndex = 0;
        private float lastAttackTime;
        public override IEnumerator Trigger()
        {
            var timeGap = Time.time - lastAttackTime;
            if (lastAttackTime != 0 && timeGap < actions[actionIndex].maxDelay && timeGap > actions[actionIndex].minDelay)
                actionIndex++;
            else actionIndex = 0;

            IsActive = true;
            StartCoroutine(actions[actionIndex].action.Trigger());
            yield return null;
        }

        public override IEnumerator Release()
        {
            IsActive = false;
            StartCoroutine(actions[actionIndex].action.Release());
            StartCoroutine(Finish());
            yield return null;
        }

        public override IEnumerator Finish()
        {
            yield return activeAction;
            activeAction = null;
            IsActive = false;
            yield return null;
        }
    }

    [SerializeField]
    public struct ActionSequenceItem
    {
        public Action action;
        public float minDelay;
        public float maxDelay;
    }
}
