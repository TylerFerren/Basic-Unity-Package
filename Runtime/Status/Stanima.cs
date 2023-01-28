using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Codesign;

public class Stanima : Status
{
    public void OnValidate()
    {
        InspectorBarColor = new Color(0.8f, 0.3f, 0.3f, 1);
    }
}
