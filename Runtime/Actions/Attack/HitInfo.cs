using System;
using UnityEngine;

namespace Codesign
{
    [Serializable]
    public struct HitInfo
    {
        public HitInfo(Action action, Collider hit, bool kill)
        {
            AttackAction = action;
            HitCollider = hit;
            Kill = kill;
        }

        public Action AttackAction { get; set; }
        public Collider HitCollider { get; set; }
        public bool Kill { get; set; }
    }
}
