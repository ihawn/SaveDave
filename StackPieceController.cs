using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackPieceController : MonoBehaviour
{
    public bool willDrop;
    Rigidbody rb;
    private StackSpawner theStackSpawner;

    private void OnEnable()
    {
        theStackSpawner = FindObjectOfType<StackSpawner>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = true;
        willDrop = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        RaycastHit hit;

        if(Physics.Raycast(transform.position, Vector3.down, out hit, theStackSpawner.stackHeight))
        {
            if(hit.collider.gameObject.tag == "ground")
            {
                willDrop = true;
            }
        }
    }
}
