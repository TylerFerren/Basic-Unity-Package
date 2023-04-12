using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    
    public class ObjectSpawner : Spawner
    {
        
        [SerializeField, PropertyOrder(-1)] private SpawnContext SpawnInfo;

        public override void Awake()
        {
            base.Awake();
            if (useObjectPool)
            { 
                foreach (SpawnItem spawnItem in SpawnInfo.itemsToSpawn)
                {
                    objectPooler.ObjectsToPool.Add(new ObjectPoolItem(spawnItem.objectToSpawn, 5, true, objectPooler));
                }
            }
        }

        public void Start()
        {
            StartCoroutine(Spawn(SpawnInfo));
        }

        public IEnumerator ObjectSpawning() {
            yield return Spawn(SpawnInfo);

            if (SpawnInfo.SpawnType == SpawnTypes.Continuous)
                yield return new WaitForSeconds(SpawnInfo.SpawnDelay);



        }
    }
}