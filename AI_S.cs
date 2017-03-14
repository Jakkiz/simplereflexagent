using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class AI_S : MonoBehaviour
{
    //Enum
    enum CellState { Clean = 0, Dirty = 1 };
    enum CleanerState { Idle = 0, Active = 1, Off = 2 };
    enum MoveDir { Up = 1, Down = 2, Left = 3, Right = 4 };

    //Variables 
    private CellState cellState;
    private CleanerState cleanerState;
    private MoveDir movingDirection;
    private bool succesfull = false; //tells if the last movement action has been succesfulls
    private int step = 0;           //keeps track of the steps ( 1 step = 1 movement)
    private int dirtPickedUp = 0;   //keeps track of the amount of dirt the AI collected
    private float distanceThreshold = 0.01f;
    bool goingLeft;
    int pickCorner = 0;
    private bool invert = false;
    //Temp variables
    private bool foundCorner;
    private bool foundWidth;
    List<MoveDir> allBoundries = new List<MoveDir>();
    List<string> boundries4 = new List<string>();
    MoveDir dir;
    // counters for width and height
    int width = 0;
    int height = 0;
    // counter used to find width. 
    int counter = -1;

    // string used to determine the moving direction during FindCorner
    private string cornerDir = "null";


    //Translation Variable 
    private Vector3 endPos;   //keep track of the starting position at the moment of the translarion begin
    public float speed = 1.0F; //sets the speed at which the Cleaner is moving

    //Collision Detection
    private float posTimer = 0;
    private Vector3 previousPos;

    void Awake()
    {
        cleanerState = CleanerState.Idle;
        previousPos = transform.position;
    }

    void Update()
    {
        switch (cleanerState)
        {
            // if Cleaner is active, Move!
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
            //if cleaner is Idle : Clean if Cell is dirty, move if cell is clean
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
            case CleanerState.Off:
                {
                    // do nothing
                    break;
                }
        }
    }

    //Move
    MoveDir Move()
    {
        step++; //increasing the step count
        //Picking direction
        //if invert is true, we arrived at the end of the room so it'll invert its pathfinding and start cleaning again
        if (invert)
        {
            Invert();

        }
        if (!foundCorner)
        {
            // function used to find a random corner
            FindCorner();
        }
        else
        {
            //Corner found, start counting how wide ambient is
            if (!foundWidth)
            {
                //depending on the random corner the AI picked, move right or left.
                if (pickCorner == 1 || pickCorner == 4)
                {
                    dir = MoveDir.Right;
                    goingLeft = false;
                }
                else
                {
                    dir = MoveDir.Left;
                    goingLeft = true;
                }
            }
            else
            {
                //we know the width, so lets go down one row
                //if counter is -1 we are at the end of a row so we wanna go down
                if (counter == -1)
                {
                    if (pickCorner == 1 || pickCorner == 2)
                    {
                        dir = MoveDir.Down;
                    }
                    else
                    {
                        dir = MoveDir.Up;
                    }
                    counter++;
                }
                //if counter is != -1, we're still traversing the row so we want to go either left or right depending on a Bool we use to pick the right direction
                else
                {
                    if (goingLeft)
                    {
                        dir = MoveDir.Right;
                    }
                    else
                    {
                        dir = MoveDir.Left;
                    }
                    //increase counter for every "horizontal" movement.
                    counter++;
                }
                //if the counter equals width, we're at the end of a row, so we want to set the counter to -1 to move vertically on the next cycle, and invert the horizontal movmenet.
                if (counter == width)
                {
                    counter = -1;
                    goingLeft = !goingLeft;
                    height = height + 1;
                }
            }
        }
        //Switch used to do the "actual" movements
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
                if (!foundWidth && foundCorner)
                {
                    width = width + 1;
                }
                break;
            case (MoveDir.Right):
                endPos = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
                if (!foundWidth && foundCorner)
                {
                    width = width + 1;
                }
                break;
        }
        return dir;
    }

    void Invert()
    {
        //set the counter to 0 to start "fresh" with a new cycle
        counter = 0;
        // check what corner we previously picked, and set the value to the opposite.
        switch (pickCorner)
        {
            case (1):
                pickCorner = 4;
                break;
            case (2):
                pickCorner = 3;
                break;
            case (3):
                pickCorner = 2;
                break;
            case (4):
                pickCorner = 1;
                break;
        }
        //set invert to false as we've inverted all the desired values!
        invert = false;
    }

    //function called if movment has been unsuccesful
    MoveDir MoveBack()
    {

        Debug.Log("Moving Back");
        //Setting the direction to be the opposite of the previous one in order to reset the target position to the starting position
        MoveDir newDir = 0;
        step++;
        switch (movingDirection)
        {
            case (MoveDir.Up):
                endPos = new Vector3(endPos.x, endPos.y, endPos.z - 1);
                newDir = MoveDir.Down;
                allBoundries.Add(MoveDir.Up);
                // if we've found both a corner and the width, and we bump vertically, it means we've traveled through the whole ambient and we need to reset.
                if (foundCorner && foundWidth)
                {
                    invert = true;
                }
                break;
            case (MoveDir.Down):
                endPos = new Vector3(endPos.x, endPos.y, endPos.z + 1);
                newDir = MoveDir.Up;
                allBoundries.Add(MoveDir.Down);
                // if we've found both a corner and the width, and we bump vertically, it means we've traveled through the whole ambient and we need to reset.
                if (foundCorner && foundWidth)
                {
                    invert = true;
                }
                break;
            case (MoveDir.Left):
                endPos = new Vector3(endPos.x + 1, endPos.y, endPos.z);
                newDir = MoveDir.Right;
                allBoundries.Add(MoveDir.Left);
                //if we've found a corner and we bump horizontally, we've found the ambient width!
                if (foundCorner && !foundWidth)
                {
                    width = width - 1;
                    foundWidth = true;
                }
                break;
            case (MoveDir.Right):
                endPos = new Vector3(endPos.x - 1, endPos.y, endPos.z);
                newDir = MoveDir.Left;
                allBoundries.Add(MoveDir.Right);
                //if we've found a corner and we bump horizontally, we've found the ambient width!
                if (foundCorner && !foundWidth)
                {
                    width = width - 1;
                    foundWidth = true;
                }
                break;
        }
        //During the function, we add to the "allBoundries" list all the collision, but we want to keep only unique values for our purpuses.
        allBoundries = allBoundries.Distinct().ToList();
        if (allBoundries.Count >= 2)
        {
            //since we move diagonally, if we have found 2 walls it means we're currently in a corner.
            foundCorner = true;
        }
        //returns the direction to "center" itself again after a collision
        return newDir;
    }

    //functions to move in the 4 directions
    void MoveUp()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    void MoveRight()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    void MoveLeft()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.right * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    void MoveDown()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.forward * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    //function called to find a random corner.
    void FindCorner()
    {
        //check if boundry list is empty.
        bool isEmpty = !allBoundries.Any();
        //if we havent picked a corner, pick one.
        if (pickCorner == 0)
        {
            pickCorner = Random.Range(1, 4);
            // 1: TOP LEFT - 2: TOP RIGHT - 3: BOT RIGHT - 4: BOT LEFT
        }

        //Depending on the corner picked AND the boundries we've found, pick the appropriate move direction to reach the corner.
        switch (pickCorner)
        {
            case (1):
                if (isEmpty)
                {
                    if (cornerDir == "horizontal")
                    {
                        dir = MoveDir.Up;
                        cornerDir = "vertical";
                    }
                    else
                    {
                        dir = MoveDir.Left;
                        cornerDir = "horizontal";
                    }
                }
                else
                {
                    MoveDir firstBoundry = allBoundries.First();
                    if (firstBoundry == MoveDir.Left)
                    {
                        dir = MoveDir.Up;
                    }
                    else
                    {
                        dir = MoveDir.Left;
                    }

                }
                break;
            case (2):
                if (isEmpty)
                {
                    if (cornerDir == "horizontal")
                    {
                        dir = MoveDir.Up;
                        cornerDir = "vertical";
                    }
                    else
                    {
                        dir = MoveDir.Right;
                        cornerDir = "horizontal";
                    }
                }
                else
                {
                    MoveDir firstBoundry = allBoundries.First();
                    if (firstBoundry == MoveDir.Right)
                    {
                        dir = MoveDir.Up;
                    }
                    else
                    {
                        dir = MoveDir.Right;
                    }

                }
                break;
            case (3):
                if (isEmpty)
                {
                    if (cornerDir == "horizontal")
                    {
                        dir = MoveDir.Down;
                        cornerDir = "vertical";
                    }
                    else
                    {
                        dir = MoveDir.Right;
                        cornerDir = "horizontal";
                    }
                }
                else
                {
                    MoveDir firstBoundry = allBoundries.First();
                    if (firstBoundry == MoveDir.Right)
                    {
                        dir = MoveDir.Down;
                    }
                    else
                    {
                        dir = MoveDir.Right;
                    }

                }
                break;
            case (4):
                if (isEmpty)
                {
                    if (cornerDir == "horizontal")
                    {
                        dir = MoveDir.Down;
                        cornerDir = "vertical";
                    }
                    else
                    {
                        dir = MoveDir.Left;
                        cornerDir = "horizontal";
                    }
                }
                else
                {
                    MoveDir firstBoundry = allBoundries.First();
                    if (firstBoundry == MoveDir.Left)
                    {
                        dir = MoveDir.Down;
                    }
                    else
                    {
                        dir = MoveDir.Left;
                    }

                }
                break;
        }
    }

    //Cleans
    private void Clean()
    {
        Debug.Log("Cleaning");
        //increase cleaning score
        dirtPickedUp++;
    }

    //Checking collision with walls by taking into account the position over time
    void CheckCollision()
    {
        posTimer = posTimer + Time.deltaTime;
        if (posTimer > (0.4f / speed))
        {
            if (Vector3.Distance(previousPos, transform.position) < 0.02)
            {
                posTimer = 0;
                movingDirection = MoveBack();
            }
            previousPos = transform.position;
            posTimer = 0;
        }
    }



    //Trigger that sees if the single cell is dirty
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Dirt")
        {
            cellState = CellState.Dirty;
        }
    }


    //if the cell we're on contains dirt, clean it by destroying dirt.
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

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Current Position : " + transform.position.ToString());
        GUI.Label(new Rect(0, 15, Screen.width, Screen.height), "Next Position : " + endPos.ToString());
        GUI.Label(new Rect(0, 30, Screen.width, Screen.height), "State : " + movingDirection.ToString());
        GUI.Label(new Rect(0, 45, Screen.width, Screen.height), "Cell State : " + cellState.ToString());
        GUI.Label(new Rect(0, 60, Screen.width, Screen.height), "Steps : " + step.ToString());
        GUI.Label(new Rect(0, 75, Screen.width, Screen.height), "Dirt Picked Up : " + dirtPickedUp.ToString());
        GUI.Label(new Rect(0, 90, Screen.width, Screen.height), "Found Corner? : " + foundCorner.ToString());
        GUI.Label(new Rect(0, 105, Screen.width, Screen.height), "Found Width? : " + foundWidth.ToString());
        GUI.Label(new Rect(0, 120, Screen.width, Screen.height), "Counter : " + counter.ToString());
        GUI.Label(new Rect(0, 150, Screen.width, Screen.height), "Corner : " + pickCorner.ToString());
    }
}