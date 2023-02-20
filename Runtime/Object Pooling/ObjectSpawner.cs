using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign
{
    public enum SpawnerType { Simple, WaveSystem }
    public enum SpawnTypes { Burst, Continuous }
    public class ObjectSpawner : MonoBehaviour
    {
        #region Fields
        [SerializeField, EnumToggleButtons, HideLabel] private SpawnerType spawnerType;

        [SerializeField, ShowIf("spawnerType", SpawnerType.Simple)] private SpawnContext SpawnInfo;

        [SerializeField, DisplayAsString, ShowIf("spawnerType", SpawnerType.WaveSystem)] private int currentWave = 0;
        [SerializeField, ShowIf("spawnerType", SpawnerType.WaveSystem)] private List<SpawnContext> waves;
        [SerializeField, ShowIf("spawnerType", SpawnerType.WaveSystem)] private bool waitTillEmpty;
        [SerializeField, ShowIf("spawnerType", SpawnerType.WaveSystem), Tooltip("Call 'StartNextWave' to trigger next wave")] private bool waitForAction;
        [SerializeField, ShowIf("spawnerType", SpawnerType.WaveSystem)] private bool waitForTime;
        [SerializeField, ShowIf("waitForTime")] private float waveTimeDelay;

        [Space]
        [Title("Spawner Settings")]

        [SerializeField] private List<GameObject> spawnedObjects = new List<GameObject>();
        [SerializeField, Tooltip("Max number of objects that can be actively spawned in the scene at one time.")] private int maxSpawnAmount = 5;
        [SerializeField] private Collider spawnArea;
        [SerializeField] private bool spawnWithParentTransform;
        [SerializeField] private bool useObjectPool;
        [SerializeField, ShowIf("useObjectPool")] private ObjectPooler objectPooler;

        #region Randomization

        [SerializeField, FoldoutGroup("Randomization")] private bool randomizeRotation;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeRotation"), Indent] private Vector3 rotationMin = Vector3.one * -180;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeRotation"), Indent] private Vector3 rotationMax = Vector3.one * 180;
        [SerializeField, FoldoutGroup("Randomization")] private bool randomizeScale;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeScale"), Indent] private bool preserveAspect;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("preserveAspect"), Indent] private Vector2 scaleMinMax = new Vector2(0, 1);
        [SerializeField, FoldoutGroup("Randomization"), HideIf("preserveAspect"), ShowIf("randomizeScale"), Indent] private Vector3 scaleMin = Vector3.zero;
        [SerializeField, FoldoutGroup("Randomization"), HideIf("preserveAspect"), ShowIf("randomizeScale"), Indent] private Vector3 scaleMax = Vector3.one;
        #endregion

        public int MaxSpawnAmount { get { return maxSpawnAmount; } private set { } }
        public List<GameObject> CurrentSpawnedObjects { get { return spawnedObjects; } private set { } }
        private bool NextWave;

        #endregion

        public void Awake()
        {
            if (!useObjectPool) return;
            if (objectPooler == null)
            {
                if (TryGetComponent(out ObjectPooler pool)) objectPooler = pool;
                else objectPooler = gameObject.AddComponent<ObjectPooler>();
            }

            if (spawnerType == SpawnerType.WaveSystem)
            {
                foreach (SpawnContext context in waves)
                {
                    foreach (GameObject _object in context.objectsToSpawn)
                    {
                        objectPooler.ObjectsToPool.Add(new ObjectPoolItem(_object, 5, true, objectPooler));
                    }
                }
            }
            else
            {
                foreach (GameObject _object in SpawnInfo.objectsToSpawn)
                {
                    objectPooler.ObjectsToPool.Add(new ObjectPoolItem(_object, 5, true, objectPooler));
                }
            }
        }

        public void Start()
        {
            if (spawnArea == null) spawnArea = GetComponent<Collider>();


            if (spawnerType == SpawnerType.WaveSystem)
                StartCoroutine(WaveCycle());
            else
            {
                StartCoroutine(SpawnEvent(SpawnInfo));
            }
        }

        public void Update()
        {
            spawnedObjects.RemoveAll(item => item == null || !item.activeInHierarchy);
        }

        public IEnumerator SpawnEvent(SpawnContext context)
        {
            for (int i = 0; i < context.spawnAmount; i++)
            {
                SpawnObject(context.objectsToSpawn);

                if (spawnedObjects.Count >= maxSpawnAmount) break;

                if (context.SpawnType == SpawnTypes.Continuous)
                    yield return new WaitForSeconds(context.SpawnDelay);
            }
            if (context.SpawnType == SpawnTypes.Continuous)
                StartCoroutine(SpawnWaiter());
        }

        private IEnumerator SpawnWaiter()
        {

            while (spawnedObjects.Count >= maxSpawnAmount)
            {
                spawnedObjects.RemoveAll(item => item == null);
                yield return new WaitForSeconds(0.25f);
            }
            StartCoroutine(SpawnEvent(SpawnInfo));
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

                yield return SpawnEvent(waves[currentWave]);
                currentWave++;
            }
        }

        private void SpawnObject(List<GameObject> objectsToSpawn)
        {
            int objectSeed = UnityEngine.Random.Range(0, objectsToSpawn.Count);

            Vector3 position = RandomLocation();
            Vector3 size = objectsToSpawn[objectSeed].GetComponent<Collider>().bounds.size;
            while (!Physics.OverlapBox(position, size).Contains(spawnArea) && Physics.CheckBox(position, size, objectsToSpawn[objectSeed].transform.rotation, ~0, QueryTriggerInteraction.Ignore))
            {
                position = RandomLocation();
            }

            Quaternion rotation = randomizeRotation ? RandomRotaion() : Quaternion.identity;

            GameObject spawn = null;
            if (useObjectPool)
            {
                spawn = objectPooler.GetPooledObject(objectsToSpawn[objectSeed]);
                spawn.transform.SetPositionAndRotation(position, rotation);
            }
            else if (spawn == null)
            {
                spawn = Instantiate(objectsToSpawn[objectSeed], position, rotation, spawnWithParentTransform ? transform : null);
            }
            if (randomizeScale)
            {
                if (preserveAspect)
                    spawn.transform.localScale = Vector3.one * UnityEngine.Random.Range(scaleMinMax.x, scaleMinMax.y);
                else
                    spawn.transform.localScale = new Vector3(UnityEngine.Random.Range(scaleMin.x, scaleMax.x), UnityEngine.Random.Range(scaleMin.y, scaleMax.y), UnityEngine.Random.Range(scaleMin.z, scaleMax.z));
            }
            spawnedObjects.Add(spawn);
        }

        private Vector3 RandomLocation()
        {
            return new Vector3(
                        UnityEngine.Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                        UnityEngine.Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
                        UnityEngine.Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z));
        }

        private Quaternion RandomRotaion()
        {
            return Quaternion.Euler(
                    UnityEngine.Random.Range(rotationMin.x, rotationMax.x),
                    UnityEngine.Random.Range(rotationMin.y, rotationMax.y),
                    UnityEngine.Random.Range(rotationMin.z, rotationMax.z));
        }

        public void SetMaxSpawnAmount(int newSpwanAmount)
        {
            maxSpawnAmount = newSpwanAmount;
        }

        public void StartNextWave()
        {
            NextWave = true;
        }
    }

    [System.Serializable]
    public struct SpawnContext
    {
        public List<GameObject> objectsToSpawn;
        public SpawnTypes SpawnType;
        [ShowIf("SpawnType", SpawnTypes.Burst)] public int spawnAmount;
        [Tooltip("Delay in seconds between spawns"), ShowIf("SpawnType", SpawnTypes.Continuous)] public float SpawnDelay;
    }

}