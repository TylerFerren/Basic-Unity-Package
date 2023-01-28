using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Codesign.Collections
{
    [CreateAssetMenu(fileName = "new Collection", menuName = "Collection")]
    public class Collection : SerializedScriptableObject
    {
        public static List<Collection> collections;
        public Dictionary<CollectionObject, int> collectionObjects = new Dictionary<CollectionObject, int>();

    }
}