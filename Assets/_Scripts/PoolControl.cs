using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//single slot identifying how many of what object to pool
[System.Serializable]
public class PoolConstructor
{
    public GameObject slotObject;
    public int slotAmount;
}

//rate of spawn for object categories
[System.Serializable]
public class SpawnRate
{
    public int[] totalRate;
    public int[] enemyRate;
    public int[] collectRate;
}

public class PoolControl : MonoBehaviour 
{
    //location in which to spawn objects
    public Transform spawner;
    //local position of spawner to parent
    private Vector3 localPos;
    //initial delay before any object spawns
    [SerializeField] float spawnDelay = 1.5f;
    //time between each spawn
    [SerializeField] float spawnTime = 0.1f;
    //distance from local zero on x-axis that objects can spawn
    [SerializeField] float spread = 30;
    //length of first pool, used to set appropriate index for spawn
    private int additive;
    //current level of difficulty
    private int level = 0;
    //spawn rates of increasing difficulty
    [SerializeField] List<SpawnRate> rates;
    //pool construction blueprints
    [SerializeField] List<PoolConstructor> objectPool;
    [SerializeField] List<PoolConstructor> effectPool;
    //pool of pooled instantiated objects
    private List<List<GameObject>> spawnPools = new List<List<GameObject>>();
    private List<List<GameObject>> effectPools = new List<List<GameObject>>();

    void Start()
    {
        //store initial position of spawner relative to parent (main camera)
        localPos = spawner.localPosition;

        if(objectPool != null)
        {
            //fills pool of pools with collision objects
            InitialPool(objectPool.Count, objectPool, spawnPools);
            //fills pool of pools with particle effects
            InitialPool(effectPool.Count, effectPool, effectPools);  

            additive = rates[0].enemyRate.Length;

            InvokeRepeating("DetermineSpawn", spawnDelay, spawnTime);
        }
    }

    private void InitialPool(int size, List<PoolConstructor> toPool, List<List<GameObject>> poolTo)
    {
        for(int x = 0; x < size; x++)
        {            
            poolTo.Add(new List<GameObject>());

            for(int y = 0; y < toPool[x].slotAmount; y++)
            {
                GameObject obj = (GameObject)Instantiate(toPool[x].slotObject);
                obj.SetActive(false);
                poolTo[x].Add(obj);
            }
        }
    }

    private void DetermineSpawn()
    {
        //stop object spawning on game over
        if(GameControl.control.gameOver) CancelInvoke();

        //choose a random position relative to local position of spawner
        spawner.localPosition = localPos;
        Vector2 pos = new Vector3(spawner.position.x + Random.Range(-spread, spread), spawner.position.y);
        spawner.position = pos;
        
        //range of percentage for potential objects (10 == 100% chance)
        int category = Random.Range(1, 10);
        int index = 0;

        //set level of difficulty only to number of rates
        if(GameControl.control.GetChallengeLevel() < rates.Count) level = GameControl.control.GetChallengeLevel();
        
        //determine type of object to spawn
        if(category < rates[level].totalRate[0])
        {
            //object is an enemy (index 0 or 1)
            index = DetermineObject(rates[level].enemyRate);
        }
        else
        { 
            //object is a collectible (index 2, 3 or 4)
            index = DetermineObject(rates[level].collectRate) + additive;
        }
        
        Spawn(spawner, index);
    }

    private int DetermineObject(int[] rate)
    {
        //determine type of object in category
        int type = Random.Range(1, 10);
        int baseValue = 0, index = 0;

        for(int x = 0; x < rate.Length; x++)
        {
            if(type < baseValue + rate[x])
            {
                index = x;
                break;
            }
            else baseValue += rate[x];
        }

        return index;
    }

    private void Spawn(Transform spawner, int index)
    {
        for(int x = 0; x < spawnPools[index].Count; x++)
        {
            //only spawn next active object
            if(!spawnPools[index][x].activeInHierarchy)
            {
                spawnPools[index][x].transform.position = spawner.transform.position;
                spawnPools[index][x].transform.rotation = spawner.transform.rotation;
                spawnPools[index][x].SetActive(true);
                break;
            } 
        }
    }

    public void ActivateEffect(Transform location, int index)
    {
        for(int x = 0; x < effectPools[index].Count; x++)
        {
            //only spawn next active object
            if(!effectPools[index][x].activeInHierarchy)
            {
                effectPools[index][x].transform.position = location.transform.position;
                effectPools[index][x].transform.rotation = location.transform.rotation;
                effectPools[index][x].SetActive(true);
                break;
            }
        }
    }
}
