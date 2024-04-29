using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonController : MonoBehaviour
{

    public GameObject roomPrefab;
    public GameObject player1;
    public GameObject player2;
    public Camera mainCamera;
    private Dictionary<Vector2, RoomData> roomDataMap = new Dictionary<Vector2, RoomData>();
    private int roomIdCounter = 0;
    private int currentRoom = -1;
    
    public class RoomData
    {
        public Vector2 Position;
        public bool HasNorthNeighbor = false, HasSouthNeighbor = false, HasEastNeighbor = false, HasWestNeighbor = false;
        public int RoomId;
        public RoomController RoomController;
        public TileController TileController;

        public RoomData(Vector2 position, int roomId)
        {
            Position = position;
            RoomId = roomId;
        }
    }


    public void Cleanup(int minRooms, int maxRooms, int extraDifficulty)
    {
        //Reset stuff
        roomIdCounter = 0;
        currentRoom = -1;
        roomDataMap.Clear();

        // Move player back to the starting position
        player1.transform.position = new Vector3(-2.8f, 2f, -1f);
        player2.transform.position = new Vector3(-2.8f, 0f, -1f);

        // Reset camera position
        Camera.main.transform.position = new Vector3(4.1f, 0f, -12f);

        // Destroy all children of the Dungeon GameObject
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(WaitAndGenerate(minRooms, maxRooms, extraDifficulty));
    }

    IEnumerator WaitAndGenerate(int minRooms, int maxRooms, int extraDifficulty)
    {
        yield return new WaitForEndOfFrame(); // Wait for all destructions to complete
        GenerateLayout(minRooms, maxRooms, extraDifficulty);
    }

    public void GenerateLayout(int minRooms, int maxRooms, int extraDifficulty)
    {
        int totalRooms = Random.Range(minRooms, maxRooms + 1);
        int[] extraDifficulties = DistributeDifficulty(extraDifficulty, totalRooms);
        int currentRoomGenerated = 0;
        Debug.Log("Min Rooms: " + minRooms);
        Debug.Log("Max Rooms: " + maxRooms);
        Debug.Log("Rooms: " + totalRooms);

        //GenerateRoomAt(startPosition, 0);
        Vector2 startPosition = Vector2.zero; // Starting room at (0, 0)
        GenerateRoomAt(startPosition, extraDifficulties[0]);
        currentRoomGenerated++;

        RoomData firstRoomData;
        if (roomDataMap.TryGetValue(startPosition, out firstRoomData))
        {
            if (firstRoomData.TileController != null)
            {
                firstRoomData.TileController.ActivateRoom(true);
            }
        }

        // Set current room
        RoomData neighbor;
        if (roomDataMap.TryGetValue(new Vector2(0, 0), out neighbor))
        {
            currentRoom = neighbor.RoomId; // Storing the ID of the initial room
        }
        currentRoom = neighbor.RoomId;

        for (int i = 1; i < totalRooms; i++)
        {
            Vector2 newPosition = ChooseNewPosition();
            GenerateRoomAt(newPosition, extraDifficulties[i]);
            //GenerateRoomAt(newPosition, 0);
            currentRoomGenerated++;
        }

        AssignIllusoryWalls();
    }

    private int[] DistributeDifficulty(int totalExtraDifficulty, int numberOfRooms)
    {
        int[] difficulties = new int[numberOfRooms];
        float sum = 0;

        // Generate initial random values
        for (int i = 0; i < numberOfRooms; i++)
        {
            // Sum multiple random values to approximate a normal distribution
            difficulties[i] = Random.Range(0, (totalExtraDifficulty / numberOfRooms) + 1) + Random.Range(0, (totalExtraDifficulty / numberOfRooms)+ 1);
            sum += difficulties[i];
            //Debug.Log("Difficulty: " + difficulties[i] + " - In room " + i + " - With Sum " + sum);
        }

        // Normalize the values to ensure they sum up to totalExtraDifficulty
        float normalizationFactor;
        if (totalExtraDifficulty != 0){
            normalizationFactor = totalExtraDifficulty / sum;
        } else {
            normalizationFactor = 0;
        }
        int actualSum = 0; // To track the sum of normalized values

        for (int i = 0; i < numberOfRooms; i++)
        {
            difficulties[i] = Mathf.RoundToInt(difficulties[i] * normalizationFactor);
            actualSum += difficulties[i];
        }

        
        // Adjust for any rounding errors by randomly distributing them
        int error = totalExtraDifficulty - actualSum;
        while (error != 0)
        {
            int index = Random.Range(0, numberOfRooms);  // Pick a random room to adjust
            if (error > 0)
            {
                difficulties[index]++;
                error--;
            } 
            else if (error < 0 && difficulties[index] > 0)  // Prevent any room from going negative
            {
                difficulties[index]--;
                error++;
            } else {
                break;
            }
        }

        return difficulties;
    }

    private void GenerateRoomAt(Vector2 position, int extraDifficulty)
    {
        if (!roomDataMap.ContainsKey(position))
        {
            //Initialise
            GameObject roomInstance = Instantiate(roomPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity, transform);
            var newRoomData = new RoomData(position, roomIdCounter);
            roomIdCounter++;

            //Add Room Controller
            RoomController roomController = roomInstance.GetComponentInChildren<RoomController>();
            newRoomData.RoomController = roomController;

            //Pass Difficulty to TileController
            TileController tileController = roomInstance.GetComponentInChildren<TileController>();
            newRoomData.TileController = tileController;
            //Debug.Log("TileController ID: " + newRoomData.RoomId);
            tileController.SetRoomID(newRoomData.RoomId);
            tileController.SetDifficulty(2 + extraDifficulty);

            //Add to class map
            roomDataMap.Add(position, newRoomData);

            //Set Id of RoomController
            roomController.SetRoomID(newRoomData.RoomId);

            // Give AI current room ID
            if (roomDataMap.Count <= 1){
                Debug.Log("Initial AI Room ID set: " + newRoomData.RoomId);
                if (player2.GetComponent<AIController>().enabled){
                    player2.GetComponent<AIController>().SetRoomID(newRoomData.RoomId);
                }
            }
        }
    }

    private Vector2 ChooseNewPosition()
    {
        //Select Room to attach to
        Vector2 basePosition = Vector2.zero;
        List<Vector2> keys = new List<Vector2>(roomDataMap.Keys);
        basePosition = keys[Random.Range(0, keys.Count)];

        // Randomly choose a direction
        int direction = Random.Range(0, 4); // 0: North, 1: South, 2: East, 3: West
        Vector2 offset = Vector2.zero;
        switch (direction)
        {
            case 0: offset = new Vector2(0, 10); break; // North
            case 1: offset = new Vector2(0, -10); break; // South
            case 2: offset = new Vector2(18, 0); break; // East
            case 3: offset = new Vector2(-18, 0); break; // West
        }

        //Location of new room
        Vector2 newPosition = basePosition + offset;

        //Checks if room is already there
        if (roomDataMap.ContainsKey(newPosition))
        {
            newPosition = ChooseNewPosition();
        }

        return newPosition;
    }

    private void AssignIllusoryWalls()
    {
        // Get furthest room to set it as last room
        RoomData furthestRoom = null;
        int furthestRoomDistance = 0;

        // Synchronise Illusory Walls
        foreach (var roomData in roomDataMap.Values){

            //Calculate if there are neighbors in each direction & get illusory wall indeces
            int northernNeighborIndex = -1;
            int easternNeighborIndex = -1;
            int southernNeighborIndex = -1;
            int westernNeighborIndex = -1;
            RoomData neighbor;
            if (roomDataMap.TryGetValue(new Vector2(roomData.Position.x, roomData.Position.y + 10), out neighbor))
            {
                roomData.HasNorthNeighbor = true;
                northernNeighborIndex = neighbor.RoomController.getIllusorySouthIndex(); // North neighbor's south wall
            }
            if (roomDataMap.TryGetValue(new Vector2(roomData.Position.x, roomData.Position.y - 10), out neighbor))
            {
                roomData.HasSouthNeighbor = true;
                southernNeighborIndex = neighbor.RoomController.getIllusoryNorthIndex(); // South neighbor's north wall
            }
            if (roomDataMap.TryGetValue(new Vector2(roomData.Position.x + 18, roomData.Position.y), out neighbor))
            {
                roomData.HasEastNeighbor = true;
                easternNeighborIndex = neighbor.RoomController.getIllusoryWestIndex(); // East neighbor's west wall
            }
            if (roomDataMap.TryGetValue(new Vector2(roomData.Position.x - 18, roomData.Position.y), out neighbor))
            {
                roomData.HasWestNeighbor = true;
                westernNeighborIndex = neighbor.RoomController.getIllusoryEastIndex(); // West neighbor's east wall
            }

            //Generate Illusory Walls
            roomData.RoomController.selectIllusoryWalls(roomData.HasNorthNeighbor,  northernNeighborIndex, 
                roomData.HasEastNeighbor, easternNeighborIndex,
                roomData.HasSouthNeighbor, southernNeighborIndex,
                roomData.HasWestNeighbor, westernNeighborIndex);
            if (furthestRoom == null){
                furthestRoomDistance = Mathf.Abs((int)roomData.Position.x) + Mathf.Abs((int)roomData.Position.y);
                furthestRoom = roomData;
            } else {
                int newRoomDistance = Mathf.Abs((int)roomData.Position.x) + Mathf.Abs((int)roomData.Position.y);
                if (newRoomDistance > furthestRoomDistance){
                    furthestRoomDistance = newRoomDistance;
                    furthestRoom = roomData;
                }
            }
        }

        furthestRoom.RoomController.MarkTransitionWall(furthestRoom.HasNorthNeighbor, furthestRoom.HasEastNeighbor,
            furthestRoom.HasSouthNeighbor, furthestRoom.HasWestNeighbor);
    }

    public void HandleRoomChange(int newRoomID, GameObject activePlayer)
    {
        //Find current room
        RoomData currentRoomData = null;
        foreach (var data in roomDataMap.Values)
        {
            if (data.RoomId == currentRoom)
            {
                currentRoomData = data;
                break;
            }
        }

        // Define offsets corresponding to each direction
        Vector2[] directions = new Vector2[]
        {
            new Vector2(0, 10),    // North
            new Vector2(0, -10),   // South
            new Vector2(18, 0),    // East
            new Vector2(-18, 0)    // West
        };

        // Check each direction for a neighboring room that matches the newRoomID
        foreach (Vector2 direction in directions)
        {
            Vector2 neighborPosition = currentRoomData.Position + direction;
            if (roomDataMap.TryGetValue(neighborPosition, out RoomData neighbor) && neighbor.RoomId == newRoomID)
            {
                // If the new room ID matches the neighbor's ID, move the camera
                ShiftCameraByOffset(direction);

                // Determine the other player to move
                //GameObject otherPlayer = (activePlayer == player1) ? player2 : player1;
                //Vector3 playerOffset = new Vector3(direction.x + 18, direction.y + 10, 0); // Adjusting to correct room dimensions
                //otherPlayer.transform.position += playerOffset;

                // Determine the other player to move
                GameObject otherPlayer = (activePlayer == player1) ? player2 : player1;
                otherPlayer.transform.position += new Vector3(direction.x, direction.y, 0); // Apply only the necessary offset

                // Activate/Deactivate spawners
                if (currentRoomData.TileController != null)
                {
                    currentRoomData.TileController.ActivateRoom(false);
                }
                if (neighbor.TileController != null)
                {
                    neighbor.TileController.ActivateRoom(true);
                }

                currentRoom = newRoomID;
                return; // Exit after handling the change to avoid unnecessary checks
            }
        }
    }

    private void ShiftCameraByOffset(Vector2 offset)
    {
        if (mainCamera != null)
        {
            mainCamera.transform.Translate(new Vector3(offset.x, offset.y, 0), Space.World);
        }      
    }
}
