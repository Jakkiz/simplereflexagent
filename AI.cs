using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    //Enum
    enum CellState { Clean = 0, Dirty = 1};
    enum CleanerState { Idle = 0, Active = 1};
    enum MoveDir { Up = 1, Down = 2, Left = 3, Right = 4 };

    //Variables 
    private CellState cellState;
    private CleanerState cleanerState;
    private MoveDir movingDirection;
    private bool succesfull = false; //tells if the last movement action has been succesfulls
    private bool active = false;
    private int step = 0;
    private int dirtPickedUp = 0;

    //Translation Variable 
    private Vector3 endPos;   //keep track of the starting position at the moment of the translarion begin
    private float speed = 1.0F; //sets the speed at which the Cleaner is moving
    private float distanceThreshold = 0.01f;

    //Collision Detection
    private float posTimer = 0;
    private Vector3 previousPos;

    void Start ()
    {
        Debug.Log("Start");
        cleanerState = CleanerState.Idle;
        previousPos = transform.position;
    }
	
	void FixedUpdate ()
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

    //Move
    private MoveDir Move()
    {
        //Picking direction
        MoveDir dir = (MoveDir)Random.Range(1, 5);
        Debug.Log("Step++");
        step++;
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
        return dir;
    }

    private MoveDir MoveBack()
    {
        Debug.Log("Moving Back (Currently Adding Step?)");
        //Setting the direction to be the opposite of the previous one in order to reset the target position to the starting position
        MoveDir newDir = 0;
        step++;
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
        return newDir;
    }

    //Movement operation SET
    private void MoveUp()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveRight()
    {
        if(Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveLeft()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.right * Time.deltaTime * speed, Space.World);
        }
        else
            cleanerState = CleanerState.Idle;
    }

    private void MoveDown()
    {
        if (Vector3.Distance(endPos, transform.position) > distanceThreshold)
        {
            transform.Translate(-Vector3.forward * Time.deltaTime * speed, Space.World);
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
        posTimer = posTimer + Time.deltaTime ;
        if(posTimer > (0.4f/speed))
        {
            if(Vector3.Distance(previousPos, transform.position) < distanceThreshold)
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
        if(other.tag == "Dirt")
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
        GUI.Label(new Rect(0, 30, Screen.width, Screen.height), "Direction : " + movingDirection.ToString());
        GUI.Label(new Rect(0, 45, Screen.width, Screen.height), "Cell State : " + cellState.ToString());
        GUI.Label(new Rect(0, 60, Screen.width, Screen.height), "Steps : " + step.ToString());
        GUI.Label(new Rect(0, 75, Screen.width, Screen.height), "Dirt Picked Up : " + dirtPickedUp.ToString());
    }
}
