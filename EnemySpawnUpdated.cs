using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnUpdated : MonoBehaviour
{
    private const int SIZE = 9;

    public GameObject[] enemyPrefabs = new GameObject[SIZE];
    public int amountInLine = 1;
    public bool noReapiting;
    public float delayBetweenWaves;

    Transform spawnTransform;
    bool isActivated;
	float lastTimeActivated = Mathf.NegativeInfinity;
    
    void Start()
    {
        spawnTransform = GetComponent<Transform>();
        isActivated = false;
    }

    void OnTriggerStay(Collider other)
    {
    	if (other.tag == "Player" && !isActivated)
    	{
    		CreateEnemyWave();
    	}
    }

    void Update()
    {
    	if (Time.time > lastTimeActivated + delayBetweenWaves && !noReapiting)
    	{
    		isActivated = false;
    	}
    }

    void CreateEnemyWave()
    {
    	int j = 0;
    	int k = 0;
    	for (int i = 0; i < enemyPrefabs.Length; i++)
    	{
    		if (i > 0 && i % amountInLine == 0)
    		{
    			k += 1;
    			j = 0;
    		}

    		Vector3 position = new Vector3(spawnTransform.position.x + 3 * j,
    									   spawnTransform.position.y,
    									   spawnTransform.position.z - 3 * k);
    		Instantiate(enemyPrefabs[i], position, spawnTransform.rotation);
    		j += 1;
    	}
        
    	isActivated = true;
    	lastTimeActivated = Time.time;
    	
    	if (noReapiting)
    	{
    		Destroy(this.gameObject);
    	}
    }
}

