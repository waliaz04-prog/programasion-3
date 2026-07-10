using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class CollectablesSpawn : MonoBehaviour
{
    public static CollectablesSpawn Instance;

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
                int randomObject = Random.Range(0, 100);
                int queueIndex;
                if (randomObject < 60)
                {
                    queueIndex = 0;
                }
                else if (randomObject < 90)
                {
                    queueIndex = 1;
                }
                else
                {
                    queueIndex = 2;
                }

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
