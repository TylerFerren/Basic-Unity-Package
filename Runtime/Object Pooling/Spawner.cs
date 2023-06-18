using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Codesign {
    public enum SpawnerType { Simple, WaveSystem }
    public enum SpawnTypes { Burst, Continuous }
    public abstract class Spawner : MonoBehaviour
    {
        [Space]
        [SerializeField] protected List<GameObject> spawnedObjects = new List<GameObject>();
        [SerializeField] private Bounds spawnArea;
        [SerializeField, Tooltip("Max number of objects that can be actively spawned in the scene at one time.")] protected int maxSpawnAmount = 5;
        [SerializeField] private bool spawnWithParentTransform;
        [SerializeField] protected bool useObjectPool;
        [SerializeField, ShowIf("useObjectPool")] protected ObjectPooler objectPooler;

        
        public int MaxSpawnAmount { get { return maxSpawnAmount; } private set { } }
        public List<GameObject> CurrentSpawnedObjects { get { return spawnedObjects; } private set { } }

        public virtual void Awake()
        {
            if (!useObjectPool) return;
            if (objectPooler == null)
            {
                if (TryGetComponent(out ObjectPooler pool)) objectPooler = pool;
                else objectPooler = gameObject.AddComponent<ObjectPooler>();
            }
        }

        public void Update() => spawnedObjects.RemoveAll(item => item == null || !item.activeInHierarchy);

        public IEnumerator Spawn(SpawnContext context)
        {
            for (int i = 0; i < context.spawnAmount; i++)
            {
                SpawnObject(context.itemsToSpawn);

                if (spawnedObjects.Count >= maxSpawnAmount) break;

                if (context.SpawnType == SpawnTypes.Continuous)
                    yield return new WaitForSeconds(context.SpawnDelay);
            }

            if (context.SpawnType == SpawnTypes.Continuous)
                StartCoroutine(SpawnWaiter(context));
        }

        private IEnumerator SpawnWaiter(SpawnContext context)
        {
            while (spawnedObjects.Count >= maxSpawnAmount)
            {
                spawnedObjects.RemoveAll(item => item == null);
                yield return new WaitForSeconds(0.25f);
            }
            StartCoroutine(Spawn(context));
        }
    
        protected void SpawnObject(List<SpawnItem> spawnItems)
        {
            int objectSeed = Random.Range(0, spawnItems.Count);

            var spawnItem = spawnItems[objectSeed];

            Vector3 position = RandomLocation();
            Vector3 size = spawnItem.objectToSpawn.GetComponent<Collider>().bounds.size;
            Quaternion rotation = spawnItem.randomizeRotation ? spawnItem.RandomRotaion() : Quaternion.identity;


            while (Physics.CheckBox(position, size, spawnItem.objectToSpawn.transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                position = RandomLocation();
            }

            GameObject spawnedObject = null;
            if (useObjectPool)
            {
                spawnedObject = objectPooler.GetPooledObject(spawnItem.objectToSpawn);
                spawnedObject.transform.SetPositionAndRotation(position, rotation);
            }
            else
            {
                spawnedObject = Instantiate(spawnItem.objectToSpawn, position, rotation, spawnWithParentTransform ? transform : null);
            }

            if (spawnItem.randomizeScale)
            {
                if (spawnItem.preserveAspect)
                    spawnedObject.transform.localScale = Vector3.one * Random.Range(spawnItem.scaleMinMax.x, spawnItem.scaleMinMax.y);
                else
                    spawnedObject.transform.localScale = new Vector3(
                        Random.Range(spawnItem.scaleMin.x, spawnItem.scaleMax.x),
                        Random.Range(spawnItem.scaleMin.y, spawnItem.scaleMax.y),
                        Random.Range(spawnItem.scaleMin.z, spawnItem.scaleMax.z));
            }

            spawnedObjects.Add(spawnedObject);
        }

        private Vector3 RandomLocation()
        {
            Vector3 halfExtents = spawnArea.extents / 2;
            return transform.TransformPoint(new Vector3(
                Random.Range(-halfExtents.x, halfExtents.x),
                Random.Range(-halfExtents.y, halfExtents.y),
                Random.Range(-halfExtents.z, halfExtents.z)
            )) + spawnArea.center;
        }

        public void SetMaxSpawnAmount(int newSpwanAmount)
        {
            maxSpawnAmount = newSpwanAmount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.TransformPoint(spawnArea.center), Vector3.Scale(transform.localScale, spawnArea.extents));

        }
    }

    [System.Serializable]
    public struct SpawnContext
    {
        public List<SpawnItem> itemsToSpawn;

        int spawns;

        public SpawnTypes SpawnType;
        [ShowIf("SpawnType", SpawnTypes.Burst)] public int spawnAmount;
        [Tooltip("Delay in seconds between spawns"), ShowIf("SpawnType", SpawnTypes.Continuous)] public float SpawnDelay;

    }

    [System.Serializable]
    public class SpawnItem
    {
        public GameObject objectToSpawn;

        #region Randomization
        [SerializeField, FoldoutGroup("Randomization")] public bool randomizeRotation;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeRotation"), Indent] public Vector3 rotationMin = Vector3.one * -180;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeRotation"), Indent] public Vector3 rotationMax = Vector3.one * 180;

        [SerializeField, FoldoutGroup("Randomization")] public bool randomizeScale;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("randomizeScale"), Indent] public bool preserveAspect;
        [SerializeField, FoldoutGroup("Randomization"), ShowIf("preserveAspect"), Indent] public Vector2 scaleMinMax = new Vector2(0, 1);

        [SerializeField, FoldoutGroup("Randomization"), HideIf("preserveAspect"), ShowIf("randomizeScale"), Indent] public Vector3 scaleMin = Vector3.zero;
        [SerializeField, FoldoutGroup("Randomization"), HideIf("preserveAspect"), ShowIf("randomizeScale"), Indent] public Vector3 scaleMax = Vector3.one;
        #endregion

        public Quaternion RandomRotaion()
        {
            return Quaternion.Euler(
                Random.Range(rotationMin.x, rotationMax.x),
                Random.Range(rotationMin.y, rotationMax.y),
                Random.Range(rotationMin.z, rotationMax.z)
            );
        }
    }

}
