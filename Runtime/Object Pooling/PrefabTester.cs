using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Codesign
{
    public class PrefabTester : MonoBehaviour
    {
        public GameObject Prefab;

        List<GameObject> gameObjects = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            if (!PrefabUtility.IsPartOfAnyPrefab(Prefab)) return;

            for (int i = 0; i < 5; i++)
            {
                var newPrefab = PrefabUtility.InstantiatePrefab(Prefab, transform) as GameObject;
                print(newPrefab);
                if (newPrefab != null)
                {
                    newPrefab.SetActive(false);
                    gameObjects.Add(newPrefab);
                }
            }
            StartCoroutine(PoolObjects());
        }

        public IEnumerator PoolObjects()
        {
            var count = 0;
            yield return new WaitForSeconds(1);

            while (count < gameObjects.Count)
            {
                print("birth");
                GameObject pooledObject = gameObjects.Where(p => p.activeInHierarchy == false).FirstOrDefault();
                PrefabUtility.RevertPrefabInstance(pooledObject, InteractionMode.UserAction);
                pooledObject.SetActive(true);
                float timer = 0;
                var direction = Random.onUnitSphere;
                while (timer < 1)
                {
                    pooledObject.transform.position += direction * Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }
                pooledObject.SetActive(false);
                print("Death");
                yield return null;
            }

            yield break;
        }
    }

}