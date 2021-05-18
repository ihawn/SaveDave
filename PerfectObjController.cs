using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectObjController : MonoBehaviour
{
    public Vector3 startScale, direction;
    public float minSpeed, maxSpeed, decreaseMultiplier, scaleSpeed, speedToScale, minScale, lerpSpeed, lerpSpread;
    float speed, lerpOffset, randLerpSpeed, stopSpeed;
    bool canScale, canPower;

    // Start is called before the first frame update
    void OnEnable()
    {
        randLerpSpeed = Random.Range(lerpSpeed / 4, lerpSpeed);
        transform.localScale = startScale;
        speed = Random.Range(minSpeed, maxSpeed);
        canScale = true;
        canPower = true;

        lerpOffset = Random.Range(-lerpSpread, lerpSpread);

        stopSpeed = Random.Range(speedToScale / 6, speedToScale);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);
        speed *= decreaseMultiplier;
        
        if(speed <= stopSpeed && canScale)
        {
            
            canScale = false;

            if (!UIController.powerupActive)
                StartCoroutine(LerpToPowerupBar());
            else
                StartCoroutine(ScaleDown(1f));
        }
    }

    IEnumerator LerpToPowerupBar()
    {
        //transform.eulerAngles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

        Vector3 lerpPos = new Vector3(-1.2f, UIController.powerupSliderPosition.y + lerpOffset, 1.2f);
        float dist = Vector3.Distance(lerpPos + lerpOffset * new Vector3(0f, 1f, 0f), transform.position);
        while (dist > 0.01f)
        {
            
            transform.position = Vector3.Lerp(transform.position, lerpPos + lerpOffset * new Vector3(0f,1f,0f), randLerpSpeed * Time.deltaTime);

            if(dist <= 0.1f)
            {
                transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;

                if(canPower)
                {
                    UIController.powerupSlider.transform.localScale = UIController.barContactScale*Vector3.one;
                    UIController.increasePower = true;
                    canPower = false;
                }
            }
            dist = Vector3.Distance(lerpPos + lerpOffset * new Vector3(0f, 1f, 0f), transform.position);

            yield return null;
        }

        StartCoroutine(ScaleDown(8f));
    }

    IEnumerator ScaleDown(float mult)
    {
        while(transform.localScale.x > minScale)
        {
            transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime * mult;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
