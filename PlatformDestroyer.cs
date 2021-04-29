using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDestroyer : MonoBehaviour
{
    private StackSpawner theStackSpawner;
    public float destroyHeightDifference;

    // Start is called before the first frame update
    void Start()
    {
        theStackSpawner = FindObjectOfType<StackSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        if(theStackSpawner.transform.position.y - transform.position.y >= destroyHeightDifference)
        {
            gameObject.SetActive(false);
        }
    }
}
