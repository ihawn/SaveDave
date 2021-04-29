using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimFlowController : MonoBehaviour
{
    public Animator thisAnim;

    // Start is called before the first frame update
    void Start()
    {
        thisAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
