using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreMarkerController : MonoBehaviour
{
    public GameObject[] ticks;

    public IEnumerator AddTickPhysics()
    {
        for(int i = 0; i < ticks.Length; i++)
        {
            ticks[i].AddComponent<Rigidbody>();
            yield return null;
        }
    }
}
