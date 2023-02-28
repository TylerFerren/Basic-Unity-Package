using UnityEngine;

namespace Codesign
{
    [System.Serializable]
    public class ObjectPoolItem
    {
        public GameObject objectToPool;
        public int amountToPool;
        public bool shouldExpand;
        public ObjectPooler pool;

        public ObjectPoolItem(GameObject _objectToPool, int _amountToPool, bool _shouldExpand)
        {
            amountToPool = _amountToPool;
            objectToPool = _objectToPool;
            shouldExpand = _shouldExpand;
            if (ObjectPooler.SharedInstance) {
                pool = ObjectPooler.SharedInstance;
                ObjectPooler.SharedInstance.ObjectsToPool.Add(this);
            }
            else
                Debug.LogWarning("There is no shared Object pool");
        }

        public ObjectPoolItem(GameObject _objectToPool, int _amountToPool, bool _shouldExpand, ObjectPooler _pool)
        {
            amountToPool = _amountToPool;
            objectToPool = _objectToPool;
            shouldExpand = _shouldExpand;
            pool = _pool;
            _pool.ObjectsToPool.Add(this);
        }
    }
}
