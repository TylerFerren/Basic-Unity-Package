using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign.Collections {
    [CreateAssetMenu(fileName = "new Object", menuName ="Objects")]
    public class CollectionObject : ScriptableObject
    {
        public Texture2D icon;
        public bool stackable;
        public GameObject modelPrefab;
    }
}
