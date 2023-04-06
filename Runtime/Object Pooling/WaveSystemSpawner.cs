using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    public class WaveSystemSpawner : Spawner
    {
        [SerializeField, DisplayAsString, PropertyOrder(-1)] private int currentWave = 0;
        [SerializeField, PropertyOrder(-1)] private List<SpawnContext> waves;
        [SerializeField, PropertyOrder(-1)] private bool waitTillEmpty;
        [SerializeField, Tooltip("Call 'StartNextWave' to trigger next wave"), PropertyOrder(-1)] private bool waitForAction;
        [SerializeField, PropertyOrder(-1)] private bool waitForTime;
        [SerializeField, ShowIf("waitForTime"), Indent, PropertyOrder(-1)] private float waveTimeDelay;

        private bool NextWave;

        public override void Awake() {
            base.Awake();
            foreach (SpawnContext context in waves)
            {
                foreach (SpawnItem spawnItem in context.itemsToSpawn)
                {
                    objectPooler.ObjectsToPool.Add(new ObjectPoolItem(spawnItem.objectToSpawn, 5, true, objectPooler));
                }
            }
        }

        public void Start() {
            StartCoroutine(WaveCycle());
        }

        private IEnumerator WaveCycle()
        {
            while (currentWave < waves.Count)
            {
                if (waitTillEmpty)
                    yield return new WaitUntil(() => spawnedObjects.Count == 0);

                if (waitForAction)
                    yield return new WaitUntil(() => NextWave);

                    NextWave = false;

                if (waitForTime)
                    yield return new WaitForSeconds(waveTimeDelay);

                yield return Spawn(waves[currentWave]);
                currentWave++;
            }
        }

        public void StartNextWave()
        {
            NextWave = true;
        }

    }
}
