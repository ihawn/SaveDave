using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public float timeToStop;
    public ParticleSystem[] s;
    public GameObject trackObj;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FireFlow());   
    }

    private void Update()
    {
        transform.position = trackObj.transform.position;
    }

    IEnumerator FireFlow()
    {
        yield return new WaitForSeconds(timeToStop);

        for(int i = 0; i < s.Length; i++)
        {
            var emiss = s[i].emission;
            emiss.rateOverTime = emiss.rateOverTime.constant;
            emiss.rateOverTime = 0f;
        }

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
