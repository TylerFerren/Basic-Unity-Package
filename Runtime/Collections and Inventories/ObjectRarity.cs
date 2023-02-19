using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Codesign.Collections
{
    [CreateAssetMenu(fileName = "new Object Attribute", menuName = "Attribute/Rarity Attribute")]
    public class ObjectRarity : ObjectAttributes
    {
        public Color color;
        public float chance;
        public float propetyMultiplier;
    }
}
