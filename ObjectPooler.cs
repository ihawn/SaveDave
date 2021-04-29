using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public GameObject pooledObject;

    public int pooledAmount;

    List<GameObject> pooledObjects;

    // Start is called before the first frame update
    void Start()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject); //Create platform and assign it to game object variable
            obj.SetActive(false); //Set the platform to inactive
            pooledObjects.Add(obj); //add the platform to the list
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy) //if the platform is not active in the scene
            {
                return pooledObjects[i]; //return the platform
            }
        }

        GameObject obj = (GameObject)Instantiate(pooledObject); //Create platform and assign it to game object variable
        obj.SetActive(false); //Set the platform to inactive
        pooledObjects.Add(obj); //add the platform to the list

        return obj; //We know for sure that this object is active
    }
}
