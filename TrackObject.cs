using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackObject : MonoBehaviour
{
    public GameObject whatToTrack;
    public float trackSpeed;
    public Vector3 followOffset;
    public bool shouldFollow;


    void Update()
    {
        if (shouldFollow && Vector3.Distance(transform.position, whatToTrack.transform.position + followOffset) > 0.01f)
            transform.position = Vector3.Lerp(transform.position, whatToTrack.transform.position + followOffset, trackSpeed * Time.deltaTime);
    }
}
