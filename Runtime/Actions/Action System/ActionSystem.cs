using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    [System.Serializable, Toggle("Enabled")]
    public Abstract class ActionSystem
    {
        public bool Enabled;
    }
}
