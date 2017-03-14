using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int number = 0;
    public GameObject dirtPrefab;
    float timer;
    GameObject newDirt;
    bool dirty;

    public int CellNumber
    {
        get
        {
            return number;
        }
        set
        {
            number = value;
        }
    }

    void Start()
    {
        Status.Grid.Add(gameObject);
        newDirt = Instantiate(dirtPrefab, transform.position, transform.rotation);
    }

    void Update()
    {
        timer = timer + Time.deltaTime;
        if (timer > 20)
        {
            if (dirty == false)
            {
                int rnd = Random.Range(1, 10);
                if (rnd > 7)
                {
                    newDirt = Instantiate(dirtPrefab, transform.position, transform.rotation);
                }
                timer = 0;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Dirt")
        {
            dirty = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Dirt")
        {
            dirty = false;
        }
    }
    void OnDestroy()
    {
        Status.Grid.Remove(gameObject);
    }
}