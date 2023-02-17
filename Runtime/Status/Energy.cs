using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Codesign;


public class Energy : Status
{
    public void Reset()
    {
        StatusName = "New Energy";
        inspectorBarColor = new Color(0.4f, 0.5f, 0.9f, 1);
        currentValue = MaxValue / 2;
    }

}
