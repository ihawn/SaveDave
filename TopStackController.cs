using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopStackController : MonoBehaviour
{
    private StackSpawner theStackSpawner;
    public float posLerpSpeed, scaleLerpSpeed, daveTeeteringOffset, daveGameOverOffset, colorLerpSpeed;

    public bool hasInitiatedFall;

    public GameObject startStack;

    // Start is called before the first frame update
    void Start()
    {
        hasInitiatedFall = false;
        theStackSpawner = FindObjectOfType<StackSpawner>();
    }

    // Update is called once per frame
    void Update()
    {

        transform.position = Vector3.Lerp(transform.position, new Vector3(theStackSpawner.currentPlatformCenter.x, theStackSpawner.transform.position.y, theStackSpawner.currentPlatformCenter.z), posLerpSpeed * Time.deltaTime);

        if (!theStackSpawner.gameOver)
        {
            Color col = GetComponent<Renderer>().material.GetColor("_Color");
            Color lerpCol = Color.Lerp(col, theStackSpawner.compColor, colorLerpSpeed * Time.deltaTime);
            GetComponent<Renderer>().material.SetColor("_Color", lerpCol);
            startStack.GetComponent<Renderer>().material.SetColor("_Color", lerpCol);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(theStackSpawner.currentStackWidth, theStackSpawner.stackHeight, theStackSpawner.currentStackLength), scaleLerpSpeed * Time.deltaTime);
        }

        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scaleLerpSpeed * Time.deltaTime);
        }

        if (!theStackSpawner.gameOver)
        {
            if (theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("TeeterSmall") || theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("TeeterLarge"))
            {
                theStackSpawner.dave.transform.position = transform.position + new Vector3(0, theStackSpawner.stackHeight / 2 + daveTeeteringOffset, 0);
            }
            else
            {
                theStackSpawner.dave.transform.position = transform.position + new Vector3(0, theStackSpawner.stackHeight / 2, 0);
                
            }
        }
        else
        {
            theStackSpawner.dave.transform.position = transform.position + new Vector3(0, theStackSpawner.stackHeight / 2, 0);

            if(!hasInitiatedFall)
            {
                for (int i = 0; i < theStackSpawner.davesRigidbodies.Length; i++)
                {
                    theStackSpawner.davesRigidbodies[i].isKinematic = false;
                }

                theStackSpawner.davesAnim.enabled = false;

                hasInitiatedFall = true;
            }
            
            
        }

        
    }
}
