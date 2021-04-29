using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroidSpawner : MonoBehaviour
{
    public GameObject[] astroids;
    public GameObject topStack, verticalSpawner;
    public int astroidCount, vertAstroidCount;
    public float minScale, maxScale, distanceToBeVisable, howManyPerFrame;
    GameObject[] storedAstroids;

    bool canToggle, astroidsActive;

    // Start is called before the first frame update
    void Start()
    {
        storedAstroids = new GameObject[astroidCount + vertAstroidCount];
        StartCoroutine(SpawnAstroids());

        canToggle = false;
        astroidsActive = true;
    }

    private void Update()
    {

        //activate astroids when in range
        if(transform.position.y - topStack.transform.position.y <= distanceToBeVisable && canToggle && !astroidsActive)
        {
            astroidsActive = true;
            canToggle = false;
            StartCoroutine(ActivateAstroids(true));
        }

        //deactivate astroids when out of range
        else if(canToggle && astroidsActive && transform.position.y - topStack.transform.position.y > distanceToBeVisable)
        {
            astroidsActive = false;
            canToggle = false;
            StartCoroutine(ActivateAstroids(false));
        }
    }


    IEnumerator SpawnAstroids()
    {
        Vector3 bounds = GetComponent<BoxCollider>().bounds.size;

        for(int i = 0; i < astroidCount; i++)
        {
            Vector3 loc = transform.position + new Vector3(Random.Range(-bounds.x / 2f, bounds.x / 2f), Random.Range(-bounds.y / 2f, bounds.y / 2f), Random.Range(-bounds.z / 2f, bounds.z / 2f));

            GameObject astroid = Instantiate(astroids[Random.Range(0, astroids.Length)], loc, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
            astroid.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            storedAstroids[i] = astroid;

            if(i % howManyPerFrame == 0)
                yield return null;
        }

        Vector3 boundsVert = verticalSpawner.GetComponent<BoxCollider>().bounds.size;

        for (int i = 0; i < vertAstroidCount; i++)
        {
            Vector3 loc = verticalSpawner.transform.position + new Vector3(Random.Range(-boundsVert.x / 2f, boundsVert.x / 2f), Random.Range(-boundsVert.y / 2f, boundsVert.y / 2f) + 75f, Random.Range(-boundsVert.z / 2f, boundsVert.z / 2f));

            GameObject astroid = Instantiate(astroids[Random.Range(0, astroids.Length)], loc, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
            astroid.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            storedAstroids[i + astroidCount] = astroid;

            if (i % howManyPerFrame == 0)
                yield return null;
        }

        canToggle = true;
    }

    IEnumerator ActivateAstroids(bool act)
    {
        for(int i = 0; i < storedAstroids.Length; i+=3)
        {
            storedAstroids[i].SetActive(act);
            if(i+1 < storedAstroids.Length)
                storedAstroids[i+1].SetActive(act);
            if(i+2 < storedAstroids.Length)
                storedAstroids[i+2].SetActive(act);
            if (i % howManyPerFrame == 0)
                yield return null;
        }

        canToggle = true;
    }
}
