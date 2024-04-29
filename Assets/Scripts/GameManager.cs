using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public UnityEvent OnPlayerDeath; 
    public UnityEvent OnFirstFinisher;
    public bool isPlayerAlive = true; 
    public bool firstFinisherTriggered = false;

    // Procedural generation parameters
    private int[] minRoomsPerFloor = new int[10];
    private int[] maxRoomsPerFloor = new int[10];
    private int[] difficultyPerFloor = new int[10];
    private int currentFloor = 0; // Start at floor 0 for indexing purposes
    private float floorTransitionCooldown = 1.0f; // Cooldown time in seconds
    private float lastTransitionTime = -1.0f; // Time since last transition

    public DungeonController dungeonController;
    public TextMeshProUGUI floorText;
    public GameObject player2;

    void Start()
    {
        InitialiseFloors();
        GenerateFloor();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void KillPlayer(GameObject caller)
    {
        //Debug.Log("KillPlayer called by " + caller.name);
        //Debug.Log("KillPlayer called from " + UnityEngine.StackTraceUtility.ExtractStackTrace());
        isPlayerAlive = false;
        // Trigger the OnPlayerDeath event
        OnPlayerDeath.Invoke();
    }

    public void FirstFinisher(){
        if (!firstFinisherTriggered){
            firstFinisherTriggered = true;
            OnFirstFinisher.Invoke();
        }
    }

    public void SendRoomID(int roomID, GameObject activePlayer)
    {
        dungeonController.HandleRoomChange(roomID, activePlayer);
        player2.GetComponent<AIController>().SetRoomID(roomID);
    }

    public void AdvanceFloor()
    {
        if (CanTransitionFloor())
        {
            currentFloor++;
            floorText.text = "Floor: " + (currentFloor + 1);
            Debug.Log("New Floor: " + currentFloor);
            GenerateFloor();
        }
    }

    private bool CanTransitionFloor()
    {
        // Check if enough time has elapsed since the last transition
        if (Time.time >= lastTransitionTime + floorTransitionCooldown)
        {
            lastTransitionTime = Time.time; // Reset the last transition time
            return true;
        }
        return false;
    }

    private void GenerateFloor()
    {
        if (currentFloor < minRoomsPerFloor.Length && currentFloor < maxRoomsPerFloor.Length && currentFloor < difficultyPerFloor.Length)
        {
            // Notify the DungeonController to generate the new floor with current parameters
            //if (currentFloor != 0){
                //dungeonController.Cleanup(minRoomsPerFloor[currentFloor], maxRoomsPerFloor[currentFloor], difficultyPerFloor[currentFloor]);
            //}

            dungeonController.Cleanup(minRoomsPerFloor[currentFloor], maxRoomsPerFloor[currentFloor], difficultyPerFloor[currentFloor]);

            //dungeonController.GenerateLayout(minRoomsPerFloor[currentFloor], maxRoomsPerFloor[currentFloor], difficultyPerFloor[currentFloor]);
        }
        else
        {
            Debug.LogWarning("Floor parameters are out of range or not defined for floor " + currentFloor);
        }
    }

    private void InitialiseFloors(){
        int maxBaseDifficulty = 28;  // Maximum target difficulty at the last floor
        int minBaseDifficulty = 24;  // Minimum target difficulty at the last floor
        int maxFloor = 9;            // Last floor

        for (int i = 0; i < 10; i++){
            int roomLimiter = i;
            if (i > 4){
                roomLimiter = 4;
            }
            minRoomsPerFloor[i] = roomLimiter + 1;
            maxRoomsPerFloor[i] = roomLimiter + 3;

            // Calculate the base difficulty increment per floor, aiming for maxBaseDifficulty by floor 9
            float baseDifficultyPerFloor = (maxBaseDifficulty * (float)i / maxFloor);
            float minDifficultyPerFloor = (minBaseDifficulty * (float)i / maxFloor);

            // Random component that increases with the floor number but ensures not to exceed the maximum allowed
            int randomComponent = UnityEngine.Random.Range(0, (int)(baseDifficultyPerFloor * 0.1f));  // up to 10% of the current base difficulty

            // Calculate the final difficulty ensuring it doesn't exceed the intended maximum for the floor
            difficultyPerFloor[i] = Mathf.FloorToInt(minDifficultyPerFloor + randomComponent);
            if (difficultyPerFloor[i] > maxBaseDifficulty - randomComponent) {
                difficultyPerFloor[i] = maxBaseDifficulty - randomComponent;  // Adjust if exceeding the max allowed
            }
            //Debug.Log("Floor: " + i + " - Difficulty: " + difficultyPerFloor[i]);
        } 
    }
}
