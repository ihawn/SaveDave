using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidController : MonoBehaviour
{
    public float maxTorque;

    // Start is called before the first frame update
    void OnEnable()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(0f, maxTorque), Random.Range(0f, maxTorque), Random.Range(0f, maxTorque)));
    }



}
