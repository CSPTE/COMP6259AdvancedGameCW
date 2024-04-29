using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject[] northWalls;
    public GameObject[] eastWalls;
    public GameObject[] southWalls;
    public GameObject[] westWalls;
    private int illusoryNorthIndex = -1;
    private int illusoryEastIndex = -1;
    private int illusorySouthIndex = -1;
    private int illusoryWestIndex = -1;

    private int roomID = -1;

    public void selectIllusoryWalls(bool hasNorthRoom, int northIndex, bool hasEastRoom, int eastIndex, bool hasSouthRoom, int southIndex, bool hasWestRoom, int westIndex)
    {
        if (hasNorthRoom)
        {
            illusoryNorthIndex = selectIllusorySegment(northWalls, northIndex);
        }

        if (hasEastRoom)
        {
            illusoryEastIndex = selectIllusorySegment(eastWalls, eastIndex);
        }

        if (hasSouthRoom)
        {
            illusorySouthIndex = selectIllusorySegment(southWalls, southIndex);
        }

        if (hasWestRoom)
        {
            illusoryWestIndex = selectIllusorySegment(westWalls, westIndex);
        }
    }

    private int selectIllusorySegment(GameObject[] wallSegments, int index)
    {
        int returnIndex;
        if (index == -1){
             //Select 3 segments from each side to make illusory
            int randomIndex = Random.Range(1, wallSegments.Length - 1);
            returnIndex = randomIndex;
            GameObject selectedWall = wallSegments[randomIndex];
            GameObject selectedWallLeft = wallSegments[randomIndex - 1];
            GameObject selectedWallRight = wallSegments[randomIndex + 1];
            setWallColliderToTrigger(selectedWall);
            setWallColliderToTrigger(selectedWallLeft);
            setWallColliderToTrigger(selectedWallRight);
            selectedWall.GetComponent<WallController>().SetRoomID(roomID);
            selectedWallLeft.GetComponent<WallController>().SetRoomID(roomID);
            selectedWallRight.GetComponent<WallController>().SetRoomID(roomID);
        } else {
            //Apply synchronised wall segments
            returnIndex = index;
            GameObject selectedWall = wallSegments[index];
            GameObject selectedWallLeft = wallSegments[index - 1];
            GameObject selectedWallRight = wallSegments[index + 1];
            setWallColliderToTrigger(selectedWall);
            setWallColliderToTrigger(selectedWallLeft);
            setWallColliderToTrigger(selectedWallRight);
            selectedWall.GetComponent<WallController>().SetRoomID(roomID);
            selectedWallLeft.GetComponent<WallController>().SetRoomID(roomID);
            selectedWallRight.GetComponent<WallController>().SetRoomID(roomID);
        }

        return returnIndex;
    }

    private void setWallColliderToTrigger(GameObject wall)
    {
        // Set the collider of the selected wall and its adjacent walls to trigger
        Collider2D collider = wall.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    public int getIllusoryNorthIndex() { return illusoryNorthIndex; }
    public int getIllusoryEastIndex() { return illusoryEastIndex; }
    public int getIllusorySouthIndex() { return illusorySouthIndex; }
    public int getIllusoryWestIndex() { return illusoryWestIndex; }

    public void SetRoomID(int id){
        roomID = id;
    }

    public void MarkTransitionWall(bool hasNorthRoom,  bool hasEastRoom, bool hasSouthRoom, bool hasWestRoom)
    {
        //Select direction randomly
        List<GameObject[]> availableWalls = new List<GameObject[]>();
    
        // Add walls without neighbors to the list
        if (!hasNorthRoom) availableWalls.Add(northWalls);
        if (!hasEastRoom) availableWalls.Add(eastWalls);
        if (!hasSouthRoom) availableWalls.Add(southWalls);
        if (!hasWestRoom) availableWalls.Add(westWalls);

        if (availableWalls.Count > 0)
        {
            GameObject[] selectedWallSegments = availableWalls[Random.Range(0, availableWalls.Count)];
            int randomIndex = selectedWallSegments.Length / 2;

            GameObject selectedWall = selectedWallSegments[randomIndex];
            GameObject selectedWallLeft = selectedWallSegments[randomIndex - 1];
            GameObject selectedWallRight = selectedWallSegments[randomIndex + 1];

            // Set these wall segments as the finish line
            selectedWall.GetComponent<WallController>().SetAsFinishLine();
            selectedWallLeft.GetComponent<WallController>().SetAsFinishLine();
            selectedWallRight.GetComponent<WallController>().SetAsFinishLine();
        }
    }
    
}
