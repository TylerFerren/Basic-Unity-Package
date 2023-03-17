using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign
{
    public class Stanima : Status
    {
        public void Reset()
        {
            inspectorBarColor = new Color(0.8f, 0.3f, 0.3f, 1);
            currentValue = MaxValue / 2;
        }
    }
}

