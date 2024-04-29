using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TileController : MonoBehaviour
{
    private int maxObstacles = 10;
    private int currentObstacles = 0;
    private int attempts = 0;
    private int maxAttempts = 10;
    private int roomID = -1;

    private HashSet<int> usedGroupIndices = new HashSet<int>(); // Tracks used compound group indices
    private HashSet<string> usedDirections = new HashSet<string>(); // Tracks used directions

    private List<Spawner> spawners = new List<Spawner>(); // Tracks spawners

    //TODO: remove
    public int extraDifff; //For testing purposes

    public void SetDifficulty(int difficulty)
    {
        //TODO: remove
        extraDifff = difficulty;

        // Reset obstacle counter, attempts and used group indices
        currentObstacles = 0;
        attempts = 0;
        usedGroupIndices.Clear();
        usedDirections.Clear();

        // Get all relevant tile groups
        Transform singles = transform.Find("Singles");
        Transform doubles = transform.Find("Doubles");
        Transform triples = transform.Find("Triples");
        Transform quadruples = transform.Find("Quadruples");

        // Array of these groups for easy access
        Transform[] groups = { singles, doubles, triples, quadruples };

        // Continue placing obstacles until the limit is reached or no new blocks can be placed within attempts
        while (currentObstacles < maxObstacles)
        {
            if (!PlaceObstacle(groups))
                break;
        }


        //Spawners
        // Get Relevant Group
        Transform edges = transform.Find("Edges2");
        // Allocate 2 spawners by default
        AddSpawners(edges);
        AddSpawners(edges);

        // Calculate Remaining Difficulty
        int remainingDifficulty = difficulty - 2;

        AllocateExtraDifficulty(remainingDifficulty, edges);
    }

    private bool PlaceObstacle(Transform[] groups)
    {
        while (attempts < maxAttempts)
        {
            // Choose a random group
            int groupIndex = UnityEngine.Random.Range(0, groups.Length);
            Transform group = groups[groupIndex];

            List<Transform> availableDirections = new List<Transform>();

            foreach (Transform dirGroup in group)
            {
                if (!usedDirections.Contains(dirGroup.name))
                {
                    availableDirections.Add(dirGroup);
                }
            }

            if (availableDirections.Count > 0)
            {
                Transform directionGroup = availableDirections[UnityEngine.Random.Range(0, availableDirections.Count)];

                if (directionGroup.childCount > 0)
                {
                    if (groupIndex == 0)  // Singles have unique placement rules
                    {
                        Transform tile = FindValidTile(directionGroup, groupIndex + 1);
                        if (tile != null)
                        {
                            EnablePillar(tile);
                            usedDirections.Add(directionGroup.name);
                            return true;
                        }   
                    }
                    else  // Doubles, Triples, Quadruples
                    {
                        int compoundGroupIndex = UnityEngine.Random.Range(0, directionGroup.childCount);

                        if (!usedGroupIndices.Contains(compoundGroupIndex) &&
                            !usedGroupIndices.Contains(compoundGroupIndex - 1) &&
                            !usedGroupIndices.Contains(compoundGroupIndex + 1)) // Make sure no 2 consecutive groups are selected
                        {
                            Transform compoundGroup = directionGroup.GetChild(compoundGroupIndex);
                            EnablePillarsInGroup(compoundGroup, groupIndex + 1);
                            usedGroupIndices.Add(compoundGroupIndex);
                            usedDirections.Add(directionGroup.name); 
                            return true;
                        }
                    }
                }
            }
        
            // Increment the attempt counter and try again
            attempts++;
        }

        // All attempts exhausted and no obstacle placed
        return false;
    }

    private Transform FindValidTile(Transform directionGroup, int tileCount)
    {
        // List to hold potential candidates that meet the spacing requirement
        List<Transform> validTiles = new List<Transform>();

        for (int i = 0; i < directionGroup.childCount; i += (tileCount == 1 ? 3 : 1))  // Skip singles to maintain a gap of at least 2 between any 2 singles
        {
            Transform potentialTile = directionGroup.GetChild(i);
            validTiles.Add(potentialTile);
        }

        if (validTiles.Count > 0)
            return validTiles[UnityEngine.Random.Range(0, validTiles.Count)];

        return null;
    }

    private void EnablePillar(Transform tile)
    {
        Transform pillar = tile.Find("Pillar");
        if (pillar != null)
        {
            pillar.gameObject.SetActive(true);
            currentObstacles++; // Increment by 1 for a single tile
        }
    }

    private void EnablePillarsInGroup(Transform compoundGroup, int tileCount)
    {
        foreach (Transform tile in compoundGroup)
        {
            Transform pillar = tile.Find("Pillar");
            if (pillar != null)
            {
                pillar.gameObject.SetActive(true);
            }
        }
        currentObstacles += tileCount;
    }

    private void AddSpawners(Transform edges2)
    {
        int totalAttempts = 0;
        int maxAttempts = 10;
        while (totalAttempts < maxAttempts){
            //Choose: West, North, East, South
            Transform directionGroup = edges2.GetChild(UnityEngine.Random.Range(0, edges2.childCount));
            if (directionGroup.childCount > 0)
            {
                int tileIndex = UnityEngine.Random.Range(0, directionGroup.childCount);
                Transform tile = directionGroup.GetChild(tileIndex);

                // Check if tile already has spawner enabled
                bool tileOrNeighborHasSpawner = IsSpawnerActive(tile);
                // Check neighboring tiles
               if (tileIndex > 0) // Check the previous tile
                {
                    tileOrNeighborHasSpawner = IsSpawnerActive(directionGroup.GetChild(tileIndex - 1));
                }
                if (tileIndex < directionGroup.childCount - 1) // Check the next tile
                {
                    tileOrNeighborHasSpawner = IsSpawnerActive(directionGroup.GetChild(tileIndex + 1));
                }
                // If neither the selected tile nor the neighbors have an active spawner, activate this spawner
                if (!tileOrNeighborHasSpawner)
                {
                    Transform spawnerTransform = tile.Find("Spawner");
                    spawnerTransform.gameObject.SetActive(true);
                    spawners.Add(spawnerTransform.GetComponentInChildren<Spawner>());

                    Spawner spawnerScript = spawnerTransform.gameObject.GetComponent<Spawner>();
                    //Debug.Log("Spawner ID: " + roomID);
                    spawnerScript.SetRoomID(roomID);

                    return;
                }
            }
            totalAttempts++;
        }
    }

    private bool IsSpawnerActive(Transform tile)
    {
        Transform spawnerTransform = tile.Find("Spawner");
        return spawnerTransform != null && spawnerTransform.gameObject.activeSelf;
    }

    private void AllocateExtraDifficulty(int remainingDifficulty, Transform edges)
    {
        while (remainingDifficulty > 0)
        {
            List<Tuple<int, Action>> potentialChanges = new List<Tuple<int, Action>>();
            int currentTotalDifficulty = CalculateTotalSystemDifficulty(null, false, false);

            bool changePossible = true;
            int i = 1;
            // Evaluate potential changes for each spawner
            foreach (var spawner in spawners)
            {
                // Simulate increasing spawn rate difficulty and calculate new system difficulty
                int diffIfIncreasedRate = CalculateTotalSystemDifficulty(spawner, true, false);
                if (diffIfIncreasedRate < 45){
                    potentialChanges.Add(new Tuple<int, Action>(diffIfIncreasedRate, () => spawner.IncreaseSpawnRateDifficulty()));
                }
                //Debug.Log("Spawner: " + i + " - Increase Rate: true - Increase Max: false - Difficulty: " + diffIfIncreasedRate);

                // Simulate increasing max enemies and calculate new system difficulty
                int diffIfIncreasedMax = CalculateTotalSystemDifficulty(spawner, false, true);
                if (diffIfIncreasedMax < 45){
                    potentialChanges.Add(new Tuple<int, Action>(diffIfIncreasedMax, () => spawner.IncreaseMaxEnemies()));
                }
                //Debug.Log("Spawner: " + i + " - Increase Rate: false - Increase Max: true - Difficulty: " + diffIfIncreasedMax);

                i++;
            }

            // Consider adding a new spawner
            if (spawners.Count < 5)
            {
                int diffIfAddedSpawner = currentTotalDifficulty + ((spawners.Count + 1) * spawners.Count * 3) - 1;
                if (diffIfAddedSpawner < 45){
                    potentialChanges.Add(new Tuple<int, Action>(diffIfAddedSpawner, () => AddSpawners(edges)));
                }
                //Debug.Log("New Spawner - Difficulty: " + diffIfAddedSpawner);
            }

            if (potentialChanges.Count > 0){
                // Sort by difficulty increase and select the top three with the lowest difficulty increase
                potentialChanges.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                var chosenChange = potentialChanges.Take(Math.Min(3, potentialChanges.Count)).ToList();

                // Randomly pick one of the top three changes
                var action = chosenChange[UnityEngine.Random.Range(0, chosenChange.Count)].Item2;
                action.Invoke(); // Apply the chosen change
                remainingDifficulty--;
            } else {
                changePossible = false;
            }

            if (!changePossible){
                break;
            }
        }
    }

    private int CalculateTotalSystemDifficulty(Spawner spawnerToModify, bool increaseRate, bool increaseMax)
    {
        int totalDifficulty = 0;
        foreach (var spawner in spawners)
        {
            int spawnRateDifficulty = spawner.spawnRateDifficulty;
            int maxEnemies = spawner.maxEnemies;

            if (spawner == spawnerToModify)
            {
                if (increaseRate) 
                {
                    spawnRateDifficulty = spawner.SimulateIncreaseSpawnRateDifficulty();
                }
                if (increaseMax) {
                    maxEnemies = spawner.SimulateIncreaseMaxEnemyDifficulty();
                }
            }

            totalDifficulty += spawnRateDifficulty * maxEnemies;
        }

        return totalDifficulty;
    }

    public void ActivateRoom(bool isActive)
    {
        foreach (var spawner in spawners)
        {
            spawner.SetSpawningActive(isActive);
        }
    }

    public void SetRoomID(int id){
        roomID = id;
        //Debug.Log("TileControllerI: " + roomID);
    }
}
