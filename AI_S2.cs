using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AI_S2 : MonoBehaviour
{
    //Enum
    enum CellState { Clean = 0, Dirty = 1 };
    enum CleanerState { Idle = 0, Active = 1 };
    enum MoveDir { Up = 1, Down = 2, Left = 3, Right = 4 };

    //Variables 
    private CellState cellState;
    private CleanerState cleanerState;
    private MoveDir movingDirection;
    private bool succesful;
    private bool active = false;
    private int step = 0;
    private int dirtPickedUp = 0;
    private int count = 0;
    private bool found = false;

    //Translation Variables
    private Vector3 endPos;   //keep track of the ending position at the moment of the translarion begin
    private const float SPEED = 1.0F;
    private float distanceThreshold = 0.01f;

    //Collision Detection
    private float posTimer = 0;
    private Vector3 previousPos;

    //Moves tracker lists 
    private Vector3 localVacPos;
    private Vector3 pos;
    private List<Vector3> rightMoves;
    private List<Vector3> wrongMoves;
    private List<MoveDir> movesDone;

    void Start()
    {
        cleanerState = CleanerState.Idle;
        previousPos = transform.position;
        rightMoves = new List<Vector3>();
        wrongMoves = new List<Vector3>();
        localVacPos = new Vector3(0, 0, 0);
        rightMoves.Add(localVacPos);
        movesDone = new List<MoveDir>();
    }

    void FixedUpdate()
    {
        switch (cleanerState)
        {
            case CleanerState.Active:
                switch (movingDirection)
                {
                    case (MoveDir.Up):
                        MoveUp();
                        break;
                    case (MoveDir.Down):
                        MoveDown();
                        break;
                    case (MoveDir.Left):
                        MoveLeft();
                        break;
                    case (MoveDir.Right):
                        MoveRight();
                        break;
                }
                CheckCollision(); //Checks collision with walls or other objects
                break;
            case CleanerState.Idle:
                switch (cellState)
                {
                    case CellState.Clean:
                        movingDirection = Move();
                        cleanerState = CleanerState.Active;
                        break;
                    case CellState.Dirty:
                        Clean();
                        cellState = CellState.Clean;
                        break;
                }
                break;
        }
    }

    //Decides direction to move
    private MoveDir Move()
    {
        step++;
        MoveDir dir = 0;
        bool result = false;
        count = 0;
        SaveMove(movingDirection, true);
        //Picking a direction
        while (!result)
        {
            dir = Picker();
            result = CheckIfMoveAvailable(dir, false);
            count++;
        }
        //Setting end point of translation based on the direction 
        switch (dir)
        {
            case (MoveDir.Up):
                endPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
                break;
            case (MoveDir.Down):
                endPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
                break;
            case (MoveDir.Left):
                endPos = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
                break;
            case (MoveDir.Right):
                endPos = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
                break;
        }
        //If it's a new cell the AI didnt explore yet we add it to the list
        if (found)
            movesDone.Add(dir);
        return dir;
    }

    //Picks a direction to move 
    private MoveDir Picker()
    {
        MoveDir newDir;
        for (int j = 1; j < 5; j++)
        {
            newDir = (MoveDir)j;
            found = CheckIfMoveAvailable(newDir, true);
            if (found)
                return newDir;
        }

        if (movesDone.Count > 1)
        {
            newDir = movesDone[movesDone.Count - 1];
            switch (newDir)
            {
                case (MoveDir.Up):
                    newDir = MoveDir.Down;
                    break;
                case (MoveDir.Down):
                    newDir = MoveDir.Up;
                    break;
                case (MoveDir.Left):
                    newDir = MoveDir.Right;
                    break;
                case (MoveDir.Right):
                    newDir = MoveDir.Left;
                    break;
            }
            movesDone.RemoveAt(movesDone.Count - 1);
            return newDir;
        }
        newDir = (MoveDir)Random.Range(1, 5);
        ResetLists();
        return newDir;
    }

    private void ResetLists()
    {
        rightMoves.Clear();
        rightMoves.Add(new Vector3(0, 0, 0));
        movesDone.Clear();
    }

    private void SaveMove(MoveDir dir, bool succ)
    {
        switch (dir)
        {
            case MoveDir.Up:
                localVacPos.z += 1;
                break;
            case MoveDir.Down:
                localVacPos.z -= 1;
                break;
            case MoveDir.Right:
                localVacPos.x += 1;
                break;
            case MoveDir.Left:
                localVacPos.x -= 1;
                break;
        }
        if (succ)
        {
            succesful = true;
            if (rightMoves.Contains(localVacPos))
                return;
            rightMoves.Add(localVacPos);
        }
        else
        {
            succesful = false;
            rightMoves.Remove(localVacPos);
            if (wrongMoves.Contains(localVacPos))
                return;
            wrongMoves.Add(localVacPos);
        }
    }

    //Checks if the next move is viable rejecting moves that are in the 'wrongList' and prioritizing direction that we haven't explore yet
    private bool CheckIfMoveAvailable(MoveDir dir, bool explore)
    {
        bool result = false;
        pos = localVacPos;
        switch (dir)
        {
            case MoveDir.Up:
                pos.x = localVacPos.x;
                pos.z = localVacPos.z + 1;
                break;
            case MoveDir.Down:
                pos.x = localVacPos.x;
                pos.z = localVacPos.z - 1;
                break;
            case MoveDir.Right:
                pos.x = localVacPos.x + 1;
                pos.z = localVacPos.z;
                break;
            case MoveDir.Left:
                pos.x = localVacPos.x - 1;
                pos.z = localVacPos.z;
                break;
        }
        if (!rightMoves.Contains(pos) && !wrongMoves.Contains(pos))
            return true;
        if (rightMoves.Contains(pos) && !explore)
            return true;
        return result;
    }

    //In case of collision we return to the cell where we just moved from
    private MoveDir MoveBack()
    {
        //Setting the direction to be the opposite of the previous one in order to reset the target position to the starting position
        MoveDir newDir = 0;
        step++;
        movesDone.RemoveAt(movesDone.Count - 1);
        switch (movingDirection)
        {
            case (MoveDir.Up):
                endPos = new Vector3(endPos.x, endPos.y, endPos.z - 1);
                newDir = MoveDir.Down;
                break;
            case (MoveDir.Down):
                endPos = new Vector3(endPos.x, endPos.y, endPos.z + 1);
                newDir = MoveDir.Up;
                break;
            case (MoveDir.Left):
                endPos = new Vector3(endPos.x + 1, endPos.y, endPos.z);
                newDir = MoveDir.Right;
                break;
            case (MoveDir.Right):
                endPos = new Vector3(endPos.x - 1, endPos.y, endPos.z);
                newDir = MoveDir.Left;
                break;
        }
        SaveMove(movingDirection, false);
        return newDir;
    }

    //Movement operation SET
    private void MoveUp()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * SPEED, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveRight()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.right * Time.deltaTime * SPEED, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveLeft()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.right * Time.deltaTime * SPEED, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveDown()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.forward * Time.deltaTime * SPEED, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    //Cleans
    private void Clean()
    {
        dirtPickedUp++;
    }

    //Checking collision with walls by taking into account the position over time
    private void CheckCollision()
    {
        posTimer = posTimer + Time.deltaTime;
        if (posTimer > (0.5f / SPEED))
        {
            if (Vector3.Distance(previousPos, transform.position) < distanceThreshold)
            {
                posTimer = 0;
                movingDirection = MoveBack();
            }
            previousPos = transform.position;
            posTimer = 0;
        }
    }

    //Trigger that checks if the single cell is dirty
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Dirt")
        {
            cellState = CellState.Dirty;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Dirt")
        {
            cellState = CellState.Dirty;
            Destroy(other.gameObject, 0.3f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Dirt")
        {
            cellState = CellState.Clean;
        }
    }

    //Display on screen some variables
    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Current Position : " + transform.position.ToString());
        GUI.Label(new Rect(0, 15, Screen.width, Screen.height), "Next Position : " + endPos.ToString());
        GUI.Label(new Rect(0, 30, Screen.width, Screen.height), "LocalVacPos : " + localVacPos.ToString());
        GUI.Label(new Rect(0, 45, Screen.width, Screen.height), "Direction : " + movingDirection.ToString());
        GUI.Label(new Rect(0, 60, Screen.width, Screen.height), "Cell State : " + cellState.ToString());
        GUI.Label(new Rect(0, 75, Screen.width, Screen.height), "Steps : " + step.ToString());
        GUI.Label(new Rect(0, 90, Screen.width, Screen.height), "Dirt Picked Up : " + dirtPickedUp.ToString());
        GUI.Label(new Rect(0, 105, Screen.width, Screen.height), "Pos : " + pos.ToString());
    }
}
