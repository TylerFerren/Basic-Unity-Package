using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Codesign;

public class Stanima : Status
{
    public void Reset()
    {
        StatusName = "New Stanima";
        inspectorBarColor = new Color(0.8f, 0.3f, 0.3f, 1);
        currentValue = MaxValue/2;
    }
}

