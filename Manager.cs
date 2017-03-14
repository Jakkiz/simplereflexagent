using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject AI1;
    public GameObject AI2;
    public GameObject AI3;
    public GameObject AI;
    public GameObject Room;
    public GameObject RoomWithObstacle;
    private bool activated = false;

    //Variables to instantiate the grid;
    public GameObject cellPrefab;
    public Vector3 startCellPosition;
    public int width = 8;
    public int height = 8;

    void Start()
    {
        CreateGrid();
        //Default AI is set to spawn the Random AI1 agent 
        AI = AI1;
    }

    void Update()
    {
        ActivateAI();
        InputHandler();
    }

    //Creates the grid
    private void CreateGrid()
    {
        int counter = 0;
        Status.Grid = new List<GameObject>();
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j ++)
            {
                GameObject newCell = Instantiate(cellPrefab, new Vector3 (startCellPosition.x + j, 0.2f, startCellPosition.z - i), new Quaternion());
                newCell.GetComponent<Cell>().CellNumber = counter;
                counter++;
            }
        }
    }

    private void ActivateAI()
    {
        if (!activated && Status.AI_active)
        {
            activated = true;
            AI.transform.position = new Vector3(Status.Position.x, 0.2f, Status.Position.z);
            AI.SetActive(true);
        }
    }

    private void InputHandler()
    {
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Status.AI_active = false;
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(Room.active)
            {
                Room.SetActive(false);
                RoomWithObstacle.SetActive(true);
            }
            else
            {
                Room.SetActive(true);
                RoomWithObstacle.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            AI = AI1;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AI = AI2;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            AI = AI3;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 600, Screen.width, Screen.height), "Change Agent F1,F2,F3");
        GUI.Label(new Rect(0, 615, Screen.width, Screen.height), "Drop agent with mouse left click");
        GUI.Label(new Rect(0, 630, Screen.width, Screen.height), "Reset Scene 'R'");
        GUI.Label(new Rect(0, 645, Screen.width, Screen.height), "Add/Rem Obstacles 'T'");
    }
}
