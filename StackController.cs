using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;

public class StackController : MonoBehaviour
{
    private StackSpawner theStackSpawner;
    private UIController theUIController;
    private AudioManager theAuidoManager;
    public float stackWidth, stackPieceCount, moveSpeed, constWiggleOffset, perfectObjectOffset;
    bool startMoving, canReact, movingForward, canSpawnPerfectObjects, canPlayCrack;
    public bool comingFromX;
    Vector3 moveVector;

    public GameObject stackOutline;

    public int perfectObjectMultiplier;

    private LevelController theLevelController;

    public Color col;

    public float badThreshold, mediumThreshold;

    public bool shouldMove;

    // Start is called before the first frame update
    void OnEnable()
    {
        stackOutline.SetActive(true);

        canPlayCrack = true;
        canSpawnPerfectObjects = true;
        
        theUIController = FindObjectOfType<UIController>();
        theLevelController = FindObjectOfType<LevelController>();
        theAuidoManager = FindObjectOfType<AudioManager>();

        movingForward = true;
        theStackSpawner = FindObjectOfType<StackSpawner>();

       
        transform.localScale = new Vector3(theStackSpawner.currentStackWidth, theStackSpawner.stackHeight, theStackSpawner.currentStackLength);
        

        startMoving = true;
        canReact = true;

        if(comingFromX)
        {
            moveVector = Vector3.right;
        }
        else
        {
            moveVector = Vector3.forward;
        }

        //set color
        GetComponent<Renderer>().material.SetColor("_Color", col);
        
        stackOutline.GetComponent<Renderer>().material.SetColor("_Color", theStackSpawner.invColor);
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldMove)
        {

            if (theLevelController.inGame)
            {
                if (startMoving)
                {
                    if (comingFromX)
                    {
                        if (transform.position.x >= theStackSpawner.currentPlatformCenter.x + theStackSpawner.stackOffset - theStackSpawner.constantOffset + constWiggleOffset && movingForward)
                        {
                            movingForward = false;
                            moveVector = Vector3.left;
                        }
                        else if (transform.position.x <= theStackSpawner.currentPlatformCenter.x - theStackSpawner.stackOffset + theStackSpawner.constantOffset - constWiggleOffset && !movingForward)
                        {
                            movingForward = true;
                            moveVector = Vector3.right;
                        }
                    }
                    else
                    {
                        if (transform.position.z >= theStackSpawner.currentPlatformCenter.z + theStackSpawner.stackOffset - theStackSpawner.constantOffset + constWiggleOffset && movingForward)
                        {
                            movingForward = false;
                            moveVector = Vector3.back;
                        }
                        else if (transform.position.z <= theStackSpawner.currentPlatformCenter.z - theStackSpawner.stackOffset + theStackSpawner.constantOffset - constWiggleOffset && !movingForward)
                        {
                            movingForward = true;
                            moveVector = Vector3.forward;
                        }

                    }

                    transform.Translate(moveVector * moveSpeed * Time.deltaTime);
                }



                if (canReact && Input.GetMouseButtonDown(0) && !theUIController.IsPointerOverUIObject() && !theUIController.paused && !theLevelController.inCutscene)
                {
                    theStackSpawner.davesAnim.SetBool("StartGame", true);
                    theStackSpawner.stackSpawned = false;

                    canReact = false;
                    startMoving = false;
                    theLevelController.inGame = true;

                    stackOutline.SetActive(false);

                    if (comingFromX)
                    {



                        float scale = (theStackSpawner.currentStackWidth - Mathf.Abs(transform.position.x - theStackSpawner.currentPlatformCenter.x));
                        float offset;


                        if (scale > 0)
                        {
                            float distance = Vector3.Distance(new Vector3(theStackSpawner.currentPlatformCenter.x, transform.position.y, theStackSpawner.currentPlatformCenter.z), transform.position);
                            float tol = theStackSpawner.perfectTolerance * Mathf.Pow(theStackSpawner.currentStackWidth, 2) + theStackSpawner.perfectTolerance * 6;


                            //If falls outside of perfect tolerance
                            if (distance > tol)
                            {
                                theStackSpawner.perfectsInARow = 0;
                                theStackSpawner.stackHeight = theStackSpawner.startingStackHeight;
                                if (distance > badThreshold * tol)
                                    theUIController.ShowFeedbackText("bad");
                                else if (distance > mediumThreshold * tol)
                                    theUIController.ShowFeedbackText("medium");

                                if (transform.position.x <= theStackSpawner.currentPlatformCenter.x)
                                {
                                    offset = theStackSpawner.currentPlatformCenter.x - transform.position.x - theStackSpawner.currentStackWidth / 2 + scale / 2;
                                    transform.position += new Vector3(offset, 0, 0);

                                    //Spawn break edge
                                    SpawnStackEdgeFromX(theStackSpawner.currentPlatformCenter.x - theStackSpawner.currentStackWidth / 2 - (theStackSpawner.currentPlatformCenter.x - transform.position.x),
                                        theStackSpawner.currentStackWidth - scale);
                                }
                                else
                                {
                                    offset = transform.position.x - theStackSpawner.currentPlatformCenter.x - theStackSpawner.currentStackWidth / 2 + scale / 2;
                                    transform.position -= new Vector3(offset, 0, 0);

                                    //Spawn break edge
                                    SpawnStackEdgeFromX(theStackSpawner.currentPlatformCenter.x + theStackSpawner.currentStackWidth / 2 + (transform.position.x - theStackSpawner.currentPlatformCenter.x),
                                        theStackSpawner.currentStackWidth - scale);
                                }

                                theAuidoManager.perfectSoundIndex = 0;

                                if (canPlayCrack)
                                {
                                    theAuidoManager.PlayWhipCrack();
                                    canPlayCrack = false;
                                }

                                transform.localScale = new Vector3(scale, transform.localScale.y, transform.localScale.z);

                                theStackSpawner.currentPlatformCenter = transform.position;
                                theStackSpawner.currentStackWidth = scale;
                            }

                            else
                            {
                                //snap to position
                                transform.position = new Vector3(theStackSpawner.currentPlatformCenter.x, transform.position.y, theStackSpawner.currentPlatformCenter.z);


                                if (canSpawnPerfectObjects)
                                {
                                    theUIController.ShowFeedbackText("good");
                                    theStackSpawner.wasPerfect = true;  

                                    canSpawnPerfectObjects = false;
                                    theAuidoManager.PlayPerfectSound();
                                    StartCoroutine(SpawnPerfectObjects());

                                    if (HitPerfectOnBonus())
                                        theStackSpawner.bonusStacks = true;

                                    theStackSpawner.perfectsInARow++;

                                    if (theStackSpawner.perfectsInARow > 1 && (theStackSpawner.perfectsInARow - 1) % 7 == 0)
                                    {
                                        // theStackSpawner.stackHeight += 0.25f;
                                    }
                                }
                            }
                        }

                        else
                        {
                            if (theStackSpawner.lastSpawnedStack != null)
                            {
                                GameObject s = theStackSpawner.stackEdgePool.GetPooledObject();
                                s.transform.position = theStackSpawner.lastSpawnedStack.transform.position;
                                s.transform.localScale = theStackSpawner.lastSpawnedStack.transform.localScale;
                                s.SetActive(true);
                                theStackSpawner.lastSpawnedStack.SetActive(false);


                            }
                            GameOver();
                        }
                    }

                    else
                    {

                        float scale = (theStackSpawner.currentStackLength - Mathf.Abs(transform.position.z - theStackSpawner.currentPlatformCenter.z));
                        float offset;

                        if (scale > 0)
                        {
                            float distance = Vector3.Distance(new Vector3(theStackSpawner.currentPlatformCenter.x, transform.position.y, theStackSpawner.currentPlatformCenter.z), transform.position);
                            float tol = theStackSpawner.perfectTolerance * Mathf.Pow(theStackSpawner.currentStackLength, 2) + theStackSpawner.perfectTolerance * 6;


                            //If falls outside of perfect tolerance
                            if (distance > tol)
                            {
                                theStackSpawner.perfectsInARow = 0;
                                theStackSpawner.stackHeight = theStackSpawner.startingStackHeight;
                                if (distance > badThreshold * tol)
                                    theUIController.ShowFeedbackText("bad");
                                else if (distance > mediumThreshold * tol)
                                    theUIController.ShowFeedbackText("medium");

                                if (transform.position.z <= theStackSpawner.currentPlatformCenter.z)
                                {
                                    offset = theStackSpawner.currentPlatformCenter.z - transform.position.z - theStackSpawner.currentStackLength / 2 + scale / 2;
                                    transform.position += new Vector3(0, 0, offset);

                                    //Spawn break edge
                                    SpawnStackEdgeFromX(theStackSpawner.currentPlatformCenter.z - theStackSpawner.currentStackLength / 2 - (theStackSpawner.currentPlatformCenter.z - transform.position.z),
                                        theStackSpawner.currentStackLength - scale);
                                }
                                else
                                {
                                    offset = transform.position.z - theStackSpawner.currentPlatformCenter.z - theStackSpawner.currentStackLength / 2 + scale / 2;
                                    transform.position -= new Vector3(0, 0, offset);

                                    //Spawn break edge
                                    SpawnStackEdgeFromX(theStackSpawner.currentPlatformCenter.z + theStackSpawner.currentStackLength / 2 + (transform.position.z - theStackSpawner.currentPlatformCenter.z),
                                        theStackSpawner.currentStackLength - scale);
                                }

                                theAuidoManager.perfectSoundIndex = 0;


                                if (canPlayCrack)
                                {
                                    theAuidoManager.PlayWhipCrack();
                                    canPlayCrack = false;
                                }


                                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, scale);

                                theStackSpawner.currentPlatformCenter = transform.position;
                                theStackSpawner.currentStackLength = scale;
                            }

                            else
                            {
                                //snap to position
                                transform.position = new Vector3(theStackSpawner.currentPlatformCenter.x, transform.position.y, theStackSpawner.currentPlatformCenter.z);

                                if (canSpawnPerfectObjects)
                                {
                                    theUIController.ShowFeedbackText("good");
                                    theStackSpawner.wasPerfect = true;

                                    if (HitPerfectOnBonus())
                                        theStackSpawner.bonusStacks = true;

                                    canSpawnPerfectObjects = false;
                                    theAuidoManager.PlayPerfectSound();
                                    StartCoroutine(SpawnPerfectObjects());
                                    theStackSpawner.perfectsInARow++;

                                    if (theStackSpawner.perfectsInARow > 1 && (theStackSpawner.perfectsInARow - 1) % 7 == 0)
                                    {
                                        //  theStackSpawner.stackHeight += 0.25f;
                                    }
                                }

                            }
                        }

                        else
                        {
                            GameOver();
                        }
                    }

                }
            }

        }
    }

    bool HitPerfectOnBonus()
    {
        if (Vector3.Distance(theStackSpawner.lastSpawnedStack.transform.position, theStackSpawner.bonusIndicator.transform.position) < 0.001f)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    void SpawnStackEdgeFromX(float spawnOffset, float xScale)
    {
        GameObject edge = theStackSpawner.stackEdgePool.GetPooledObject();

        if (comingFromX)
        {
            edge.transform.position = new Vector3(spawnOffset, transform.position.y, transform.position.z);
            edge.transform.localScale = new Vector3(xScale, edge.transform.localScale.y, theStackSpawner.currentStackLength);
        }
        else
        {
            edge.transform.position = new Vector3(transform.position.x, transform.position.y, spawnOffset);
            edge.transform.localScale = new Vector3(theStackSpawner.currentStackWidth, edge.transform.localScale.y, xScale);
        }
        edge.transform.rotation = transform.rotation;
        edge.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        edge.GetComponent<Renderer>().material.SetColor("_Color", col);
        edge.SetActive(true);
    }

    void GameOver()
    {

        theLevelController.vcam.LookAt = theStackSpawner.davesRagdoll.transform;
        theLevelController.davesUmbrella.AddComponent<Rigidbody>();
        var composer = theLevelController.vcam.GetCinemachineComponent<CinemachineComposer>();
        composer.m_DeadZoneWidth = theStackSpawner.cameraDeadzone.x;
        composer.m_DeadZoneHeight = theStackSpawner.cameraDeadzone.y;
        theStackSpawner.davesAnim.SetBool("GameOver", true);
        theUIController.pauseButton.SetActive(false);
        theStackSpawner.restartButton.SetActive(true);
        theStackSpawner.gameOver = true;
        theLevelController.inGame = false;
        theUIController.wasInDeathMenu = true;
    }

    IEnumerator SpawnPerfectObjects()
    {
        
        for(int i = 0; i < theStackSpawner.perfectObjectMultiplier * Mathf.Max(theStackSpawner.currentStackLength, theStackSpawner.currentStackWidth)/theStackSpawner.startingStackWidth; i++)
        {
            for (int j = 0; j < perfectObjectMultiplier; j++)
            {
                int directionSelector = Random.Range(0, 4);

                GameObject p = theStackSpawner.perfectObjPool.GetPooledObject();
                float yPos = transform.position.y - theStackSpawner.stackHeight / 2;

                switch (directionSelector)
                {
                    case 0:
                        p.transform.position = new Vector3(transform.position.x + theStackSpawner.currentStackLength / 2 - perfectObjectOffset, yPos, transform.position.z + Random.Range(-theStackSpawner.currentStackWidth / 2, theStackSpawner.currentStackLength / 2));
                        p.GetComponent<PerfectObjController>().direction = Vector3.right;
                        break;

                    case 1:
                        p.transform.position = new Vector3(transform.position.x - theStackSpawner.currentStackLength / 2 + perfectObjectOffset, yPos, transform.position.z + Random.Range(-theStackSpawner.currentStackWidth / 2, theStackSpawner.currentStackLength / 2));
                        p.GetComponent<PerfectObjController>().direction = Vector3.left;
                        break;

                    case 2:
                        p.transform.position = new Vector3(transform.position.x + Random.Range(-theStackSpawner.currentStackLength / 2, theStackSpawner.currentStackWidth / 2), yPos, transform.position.z + theStackSpawner.currentStackWidth / 2 - perfectObjectOffset);
                        p.GetComponent<PerfectObjController>().direction = Vector3.forward;
                        break;

                    case 3:
                        p.transform.position = new Vector3(transform.position.x + Random.Range(-theStackSpawner.currentStackLength / 2, theStackSpawner.currentStackWidth / 2), yPos, transform.position.z - theStackSpawner.currentStackWidth / 2 + perfectObjectOffset);
                        p.GetComponent<PerfectObjController>().direction = Vector3.back;
                        break;
                }

                p.GetComponent<Renderer>().material.SetColor("_Color", theStackSpawner.compColor);
                p.SetActive(true);
            }


            yield return null;
        }

    }
}
