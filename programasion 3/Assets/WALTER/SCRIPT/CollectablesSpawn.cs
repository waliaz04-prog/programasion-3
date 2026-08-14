using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class CollectablesSpawn : MonoBehaviour
{
    public static CollectablesSpawn Instance { get; private set; }

    [SerializeField] private CollectableObject[] collectableObjects;

    [SerializeField] private int actualObjInScene;
    [SerializeField] private int maxObjInScene;

    [SerializeField] private float spawnRate;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<Transform> usedSpawnPoints;

    private Coroutine spawnObjIE;
    private Transform collectablesParent;   
    private WaitForSeconds spawnWait;       

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        if (collectableObjects == null || collectableObjects.Length == 0)
        {
            Debug.LogError("CollectablesSpawn necesita al menos un tipo de coleccionable.", this);
            enabled = false;
            return;
        }

        spawnWait = new WaitForSeconds(spawnRate);

        Transform spawnsParent = new GameObject("Spawns").transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            spawnPoints.Add(transform.GetChild(i));
        }
        foreach (Transform spawn in spawnPoints)
        {
            spawn.parent = spawnsParent;
        }
        spawnsParent.parent = this.transform;

        for (int i = 0; i < collectableObjects.Length; i++)
        {
            collectableObjects[i].pool = new Queue<GameObject>();
        }

        collectablesParent = new GameObject("Collectables").transform;

        for (int i = 0; i < collectableObjects.Length; i++)
        {
            for (int instancedPrefabs = 0; instancedPrefabs < collectableObjects[i].poolSize; instancedPrefabs++)
            {
                GameObject spawnedCollectable = Instantiate(collectableObjects[i].collectablePrefab, collectablesParent);
                spawnedCollectable.GetComponent<Collectable>().poolIndex = i; 
                spawnedCollectable.SetActive(false);
                collectableObjects[i].pool.Enqueue(spawnedCollectable);
            }
        }

        collectablesParent.parent = this.transform;

        spawnObjIE = StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (actualObjInScene < maxObjInScene)
        {
            if (spawnPoints.Count > 0) 
            {
                int queueIndex = GetRandomCollectableIndex();

                if (collectableObjects[queueIndex].pool.Count > 0) 
                {
                    Transform randomSpawn = GetRandomSpawn();
                    GameObject collectable = GetNextObj(queueIndex);
                    collectable.transform.SetParent(randomSpawn);
                    collectable.transform.position = randomSpawn.position;

                    spawnPoints.Remove(randomSpawn);
                    usedSpawnPoints.Add(randomSpawn);

                    actualObjInScene++;
                }
            }

            yield return spawnWait;
        }
        spawnObjIE = null;
    }

    private int GetRandomCollectableIndex()
    {
        float totalWeight = 0f;
        for (int i = 0; i < collectableObjects.Length; i++)
        {
            if (collectableObjects[i].pool.Count > 0)
            {
                totalWeight += Mathf.Max(0f, collectableObjects[i].spawnRate);
            }
        }

        if (totalWeight <= 0f)
        {
            for (int i = 0; i < collectableObjects.Length; i++)
            {
                if (collectableObjects[i].pool.Count > 0)
                {
                    return i;
                }
            }

            return 0;
        }

        float selection = Random.value * totalWeight;
        for (int i = 0; i < collectableObjects.Length; i++)
        {
            if (collectableObjects[i].pool.Count == 0)
            {
                continue;
            }

            selection -= Mathf.Max(0f, collectableObjects[i].spawnRate);
            if (selection <= 0f)
            {
                return i;
            }
        }

        return collectableObjects.Length - 1;
    }

    private GameObject GetNextObj(int queue)
    {
        GameObject nextObj = collectableObjects[queue].pool.Dequeue();
        nextObj.SetActive(true);
        return nextObj;
    }

    private Transform GetRandomSpawn()
    {
        int randomSpawn = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomSpawn];
    }

    public void CollectObj(GameObject obj, int poolIndex)
    {
        if (obj == null || poolIndex < 0 || poolIndex >= collectableObjects.Length)
        {
            return;
        }

        Transform objSpawnPoint = obj.transform.parent;

        usedSpawnPoints.Remove(objSpawnPoint);
        spawnPoints.Add(objSpawnPoint);

        actualObjInScene--;

        obj.SetActive(false);
        obj.transform.SetParent(collectablesParent);

        collectableObjects[poolIndex].pool.Enqueue(obj); 
        if (spawnObjIE == null)
        {
            spawnObjIE = StartCoroutine(SpawnObjects());
        }
    }

    public void StopSpawning()
    {
        if (spawnObjIE != null)
        {
            StopCoroutine(spawnObjIE);
            spawnObjIE = null;
        }
    }
}

[Serializable]
public struct CollectableObject
{
    public string objectName;
    public GameObject collectablePrefab;
    public Queue<GameObject> pool;
    public int poolSize;
    public float spawnRate;
}
