using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // Prefab to spawn

    public int maxSpawnRate = 10;      // maxSpawnRate - spawnRateDifficulty = spawnRate. This specifies starting spawn rate of 5
    public float spawnRate = 7f;        // Time in seconds between each spawn currently
    public int spawnRateDifficulty = 3; // Spawn Rate Difficulty Currently
    public int maxEnemies = 2;          // The maximum number of enemies that can be alive at the same time currently

    private float nextSpawnTime = 3f;   // Counting to next spawn
    private int currentEnemyCount = 0;  // Counting current enemies

    private bool isSpawningActive = false;  // Controls whether spawning is active
    private int roomID = -1;
    
    void Update()
    {
        if (Time.time >= nextSpawnTime && currentEnemyCount < maxEnemies && isSpawningActive)
        {
            SpawnSkeleton();
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnSkeleton()
    {
        GameObject skeleton = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(0, 0, 0), transform);
        skeleton.transform.localScale = Vector3.one;
        MonsterController monsterScript = skeleton.GetComponent<MonsterController>();
        monsterScript.SetRoomID(roomID);
        currentEnemyCount++;
        MonsterController monsterController = skeleton.GetComponent<MonsterController>();
        if (monsterController != null)
        {
            monsterController.OnMonsterDeath += HandleSkeletonDespawn;  // Subscribe to death event
        }
    }

    private void HandleSkeletonDespawn()
    {
        currentEnemyCount--;
    }

    public void IncreaseSpawnRateDifficulty()
    {
        if (spawnRateDifficulty < maxSpawnRate - 2)
        {
            spawnRateDifficulty++;
            spawnRate = maxSpawnRate - spawnRateDifficulty;
        }
    }

    public void IncreaseMaxEnemies()
    {
        maxEnemies++;
    }

    public int SimulateIncreaseSpawnRateDifficulty()
    {
        if (spawnRateDifficulty < maxSpawnRate - 2) {
            int tempSpawnRateDiff = spawnRateDifficulty + 1;
            return tempSpawnRateDiff;
        } else {
            return int.MaxValue;
        }
    }

    public int SimulateIncreaseMaxEnemyDifficulty()
    {
        if (maxEnemies < 6) {
            int tempMaxEnemiesDiff = maxEnemies + 1;
            return tempMaxEnemiesDiff;
        } else {
            return int.MaxValue;
        }
    }

    public void SetSpawningActive(bool isActive)
    {
        isSpawningActive = isActive;
        if (isSpawningActive)
        {
            // Reset the next spawn time to avoid immediate multiple spawns
            nextSpawnTime = Time.time + 5f;
        }
    }

    public void SetRoomID(int id){
        //Debug.Log("Spawner ID: " + id);
        roomID = id;
    }
}