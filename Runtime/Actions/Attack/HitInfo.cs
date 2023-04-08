using System;
using UnityEngine;

namespace Codesign
{
    [Serializable]
    public struct HitInfo
    {
        public HitInfo(Action action, Collider hit, bool critical, bool kill)
        {
            AttackAction = action;
            HitCollider = hit;
            Critical = critical;
            Kill = kill;
        }

        public Action AttackAction { get; set; }
        public Collider HitCollider { get; set; }
        public bool Critical { get; set; }
        public bool Kill { get; set; }
    }
}
