using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectObjController : MonoBehaviour
{
    public Vector3 startScale, direction;
    public float minSpeed, maxSpeed, decreaseMultiplier, scaleSpeed, speedToScale, minScale;
    float speed;
    bool canScale;

    // Start is called before the first frame update
    void OnEnable()
    {
        transform.localScale = startScale;
        speed = Random.Range(minSpeed, maxSpeed);
        canScale = true;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);
        speed *= decreaseMultiplier;
        
        if(speed <= speedToScale && canScale)
        {
            //start to scale down
            canScale = false;
            StartCoroutine(ScaleDown());
        }
    }

    IEnumerator ScaleDown()
    {
        while(transform.localScale.x > minScale)
        {
            transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
