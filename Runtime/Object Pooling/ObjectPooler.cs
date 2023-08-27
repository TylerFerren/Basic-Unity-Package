using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Codesign
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler SharedInstance;

        public List<ObjectPoolItem> ObjectsToPool = new List<ObjectPoolItem>();

        public List<GameObject> pooledObjects = new List<GameObject>();

        public bool isSharedInstance = false;

        private bool usingPrefab;

        public void Awake()
        {
            if (isSharedInstance) SharedInstance = this;
        }

        public void Start()
        {

            foreach (ObjectPoolItem item in ObjectsToPool)
            {
                for (int i = 0; i < item.amountToPool; i++)
                {
                    AddNewPooledObject(item.objectToPool, false);
                }
            }
        }

        public GameObject GetPooledObject(GameObject prefab)
        {
            var pooledObject = FindPooledObject(prefab);
            if (pooledObject != null)
            {

                if (PrefabUtility.IsPartOfAnyPrefab(pooledObject)) PrefabUtility.RevertPrefabInstance(pooledObject, InteractionMode.UserAction);
                pooledObject.SetActive(true);
                return pooledObject;
            }

            foreach (ObjectPoolItem item in ObjectsToPool)
            {
                if (string.Equals(item.objectToPool.name, prefab.name) && item.shouldExpand)
                {
                    return AddNewPooledObject(item.objectToPool, true);
                }
            }
            print("no object");
            return null;
        }

        private GameObject FindPooledObject(GameObject objectToFind)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (pooledObjects[i].activeInHierarchy) continue;
                if (!pooledObjects[i].name.Contains(objectToFind.name)) continue;

                var pooledObject = pooledObjects[i];
                pooledObject.SetActive(true);
                pooledObject.transform.SetParent(transform);
                return pooledObject;
            }
            return null;
        }

        private GameObject AddNewPooledObject(GameObject objectToPool, bool isActive)
        {
            GameObject obj = null;
            if (PrefabUtility.IsPartOfAnyPrefab(objectToPool))
                obj = PrefabUtility.InstantiatePrefab(objectToPool) as GameObject;
            else
                obj = Instantiate(objectToPool);
            obj.transform.SetParent(transform);
            obj.SetActive(isActive);
            pooledObjects.Add(obj);
            return obj;
        }
    }
}
