using System;
using System.Collections;
using System.Collections.Generic;
using Codesign.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign.Collections
{
    //[CreateAssetMenu(fileName = "new Object Attribute", menuName = "Attribute/Standard Attribute")]
    public class ObjectAttributes : ScriptableObject
    {
        public string Name;

        public static List<ObjectAttributes> Instances;

        public void CreateInstance()
        {

            if (Instances.Contains(this)) return;

            ObjectAttributes instance = Instances.Find(p => p.Name == this.Name);

            if (instance == null)
            {
                instance = this;
                Instances.Add(instance);
            }
        }

        public void DeleteInstance(ObjectAttributes instance)
        {
            if (Instances.Contains(instance))
            {
                Instances.Remove(instance);
            }
        }
    }
}
