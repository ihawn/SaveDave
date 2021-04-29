using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplierTextAnimator : MonoBehaviour
{
    public Color[] colorTiers;
    public float minScale, maxScale, scaleSpeed;
    StackSpawner theStackspawner;
    bool scaleUp;
     

    // Start is called before the first frame update
    void Start()
    {
        scaleUp = true;
        theStackspawner = FindObjectOfType<StackSpawner>();
    }

    // Update is called once per frame
    /*void Update()
    {

    }*/

    private void Update()
    {
        if (theStackspawner.multiplier > 0)
        {
            if (scaleUp)
            {
                if (transform.localScale.x < maxScale)// + Mathf.Log(2,theStackspawner.multiplier))
                {
                    transform.localScale += scaleSpeed * Time.deltaTime * Vector3.one / Mathf.Log(2,theStackspawner.multiplier+1);
                }
                else
                {
                    scaleUp = false;
                }
            }

            if (!scaleUp)
            {
                if (transform.localScale.x > minScale)
                {
                    transform.localScale -= scaleSpeed * Time.deltaTime * Vector3.one / Mathf.Log(2,theStackspawner.multiplier+1);
                }
                else
                {
                    scaleUp = true;
                }
            }


        }
        else
        {
            scaleUp = true;
            transform.localScale = Vector3.one;
        }
    }

}
