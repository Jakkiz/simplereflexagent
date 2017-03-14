using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectStartingPoint : MonoBehaviour
{
    private MeshRenderer myMesh;

    void Start()
    {
        myMesh = GetComponent<MeshRenderer>();
        myMesh.enabled = false;
    }

    void Update()
    {
        if(myMesh.enabled && !Status.AI_active && Input.GetMouseButtonDown(0))
        {
            Status.Position = transform.position;
            Status.AI_active = true;
        }
    }

    void OnMouseEnter()
    {
        if(!Status.AI_active)
            myMesh.enabled = true;
    }

    void OnMouseExit()
    {
        myMesh.enabled = false;
    }

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Wall")
        {
            gameObject.SetActive(false);
        }
    }
}
