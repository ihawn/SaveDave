using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class StackSpawner : MonoBehaviour
{
    public float r, g, b;
    public float colParamMinValue;
    public Vector3 colorStep;
    public Color compColor, invColor;

    private Leaderboards theLeaderboard;
    private UIController theUIController;
    private LevelController theLevelController;
    private ScoreManager theScoreManager;
    private AudioManager theAudioManager;

    Vector3 restartPos;

    public int stackCount;


    public GameObject restartButton, dave, topStack, lastSpawnedStack, levelMarker;
    public ObjectPooler stackPool, stackEdgePool, perfectObjPool;
    public float timeBetweenStacks, stackHeight, startingStackHeight, stackOffset, startingStackOffset,
        currentStackWidth, currentStackLength, startingStackWidth, perfectTolerance, startingPerfectTolerance, perfectsInARow,
        constantOffset, widthToTeeterSmall, widthToTeeterLarge, danceTimeMin, 
        danceTimeMax, widthToBeNervous, perfectObjectMultiplier, daveWaitToBoard,
        shipWait, colorMinTolerance, colorMaxTolerance ,spawnLevelMarkerDistance;
    public Vector3 currentPlatformCenter, levelMarkerOffset;
    public Animator davesAnim;


    public Rigidbody[] davesRigidbodies;

    public GameObject davesRagdoll;

    float danceTime;

    public bool stackSpawned, gameOver, teetering, danceInitiated;

    public Vector2 cameraDeadzone;

    public GameObject[] levelMarkers;

    public float timeBetweenBonusStacks, bonusProbabilityStory, bonusProbabilityEndless, startVisibleDistance;
    public int bonusStackCountLevels, bonusStackCountEndless, bonusSpawnHeightMultiplierStory, bonusSpawnHeightMultiplierEndeless, multiplier, maxMultiplier;
    public bool bonusStacks, wasPerfect;
    public GameObject bonusIndicator, bonusText;

    


    // Start is called before the first frame update
    void Start()
    {
        

        startingPerfectTolerance = perfectTolerance;

        theLeaderboard = FindObjectOfType<Leaderboards>();

        bonusStacks = false;
        stackHeight = startingStackHeight;

   //     StartCoroutine(SetDeadzone());
        r = Random.Range(0, 255);
        g = Random.Range(0, 255);
        b = Random.Range(0, 255);

        topStack.GetComponent<Renderer>().material.SetColor("_Color", CompColor());
        theUIController = FindObjectOfType<UIController>();
        theScoreManager = FindObjectOfType<ScoreManager>();
        theLevelController = FindObjectOfType<LevelController>();
        theAudioManager = FindObjectOfType<AudioManager>();

        danceInitiated = false;
        danceTime = Random.Range(danceTimeMin, danceTimeMax);

        Application.targetFrameRate = 60;

        

        for (int i = 0; i < davesRigidbodies.Length; i++)
        {
            davesRigidbodies[i].isKinematic = true;
        }

        davesAnim = dave.GetComponent<Animator>();
        stackOffset = startingStackOffset;
        stackCount = 0;
        restartPos = transform.position;

        currentStackWidth = startingStackWidth;
        currentStackLength = startingStackWidth;
        stackSpawned = false;

        

        if (!PlayerPrefs.HasKey("showCutscene") || PlayerPrefs.GetInt("showCutscene") == 1)
        {
            davesAnim.SetBool("ShowCutscene", true);
        }
        else if(PlayerPrefs.HasKey("showCutscene") && PlayerPrefs.GetInt("showCutscene") == 0)
        {
            davesAnim.SetBool("ShowCutscene", false);
        }

        levelMarkers = new GameObject[theLevelController.levelRequirements.Length];

       /* var composer = theLevelController.vcam.GetCinemachineComponent<CinemachineComposer>();
        composer.m_DeadZoneWidth = 0f;
        composer.m_DeadZoneHeight = 0f;*/
    }

    // Update is called once per frame
    void Update()
    {
        if (theLevelController.inGame)
        {
            if (!danceInitiated)
            {
                StartCoroutine(Dance());
                danceInitiated = true;
            }

            //add check for mode here
            if (PlayerPrefs.GetString("mode") == "level" || !PlayerPrefs.HasKey("mode"))
            {
                if (theLevelController.level < theLevelController.levelRequirements.Length && !stackSpawned && !gameOver && theScoreManager.score < theLevelController.levelRequirements[theLevelController.level])
                {
                    stackSpawned = true;

                    if (!bonusStacks)
                        StartCoroutine(SpawnStack());
                    else
                        StartCoroutine(SpawnBonusStacks());
                }
                else if (theLevelController.level < theLevelController.levelRequirements.Length && !stackSpawned && !gameOver && theScoreManager.score == theLevelController.levelRequirements[theLevelController.level] - 0.5f)
                {
                    stackCount++;

                }
            }

            else if(PlayerPrefs.GetString("mode") == "endless")
            {
                if (!stackSpawned && !gameOver)
                {
                    stackSpawned = true;

                    if (!bonusStacks)
                        StartCoroutine(SpawnStack());
                    else
                        StartCoroutine(SpawnBonusStacks());
                }
            }

                //Control dave's progression animations
                if (currentStackLength <= widthToBeNervous || currentStackWidth <= widthToBeNervous)
            {
                teetering = true;
                davesAnim.SetBool("Nervous", true);
            }
            if (currentStackLength <= widthToTeeterSmall || currentStackWidth <= widthToTeeterSmall)
            {
                teetering = true;
                davesAnim.SetBool("TeeterSmall", true);
            }
            if (currentStackLength <= widthToTeeterLarge || currentStackWidth <= widthToTeeterLarge)
            {
                teetering = true;
                davesAnim.SetBool("TeeterLarge", true);
            }
        }

        if(bonusIndicator.activeInHierarchy)
        {
            Vector3 lerpedPos = Vector3.Lerp(bonusIndicator.transform.position, currentPlatformCenter, Time.deltaTime * 10f);
            bonusIndicator.transform.position = new Vector3(lerpedPos.x, bonusIndicator.transform.position.y, lerpedPos.z);
            bonusIndicator.transform.localScale = Vector3.Lerp(bonusIndicator.transform.localScale, new Vector3(currentStackWidth*50, currentStackLength*50, stackHeight*200 ), Time.deltaTime * 10f);
            bonusText.transform.position = bonusIndicator.transform.position + new Vector3(1.5f, 0.3f, 0);


            float alpha = 1 - Mathf.Abs(transform.position.y - bonusIndicator.transform.position.y) / startVisibleDistance;
            bonusIndicator.GetComponent<Renderer>().material.SetFloat("_Opacity", alpha);
            bonusText.GetComponent<Renderer>().material.SetFloat("_Opacity", alpha);

            if (transform.position.y > bonusIndicator.transform.position.y + stackHeight)
            {
                bonusIndicator.SetActive(false);
                bonusText.SetActive(false);
            }
        }
    }




    void Multiplier()
    {
        if(PlayerPrefs.GetString("mode") == "endless")
        {

        }
    }


    IEnumerator SpawnStack()
    {
        yield return new WaitForSeconds(timeBetweenStacks);

        stackCount++;
        CheckAndMakeStacks();
        transform.position += new Vector3(0, stackHeight, 0);

        if(PlayerPrefs.GetString("mode") == "level")
        {
            if (Random.Range(0f, 100f) <= bonusProbabilityStory && !bonusIndicator.activeInHierarchy)
            {
                bonusIndicator.SetActive(true);
                bonusText.SetActive(true);
                bonusIndicator.transform.position = new Vector3(transform.position.x, transform.position.y + Random.Range(bonusSpawnHeightMultiplierStory / 2, bonusSpawnHeightMultiplierStory) * stackHeight, transform.position.z);
            }
        }

        if(PlayerPrefs.GetString("mode") == "endless")
        {
            if (Random.Range(0f, 100f) <= bonusProbabilityEndless && !bonusIndicator.activeInHierarchy)
            {
                bonusIndicator.SetActive(true);
                bonusText.SetActive(true);
                bonusIndicator.transform.position = new Vector3(transform.position.x, transform.position.y + Random.Range(bonusSpawnHeightMultiplierEndeless / 2, bonusSpawnHeightMultiplierEndeless) * stackHeight, transform.position.z);
            }
        }

    }

    IEnumerator SpawnBonusStacks()
    {
        theAudioManager.generalSounds[1].Play();
        theAudioManager.generalSounds[1].pitch = 2.5f;
        bonusIndicator.SetActive(false);
        bonusText.SetActive(false);
        int howMany = 0;
        
        if (PlayerPrefs.GetString("mode") == "level")
            howMany = bonusStackCountLevels;

        if (PlayerPrefs.GetString("mode") == "endless")
            howMany = bonusStackCountEndless;

        for (int i = 0; i < howMany; i++)
        {
            if ((PlayerPrefs.GetString("mode") == "level" && theLevelController.levelRequirements[theLevelController.level] - theScoreManager.score > 2 * stackHeight) || PlayerPrefs.GetString("mode") == "endless" || !PlayerPrefs.HasKey("mode"))
            {
                if (!PlayerPrefs.HasKey("mode"))
                    PlayerPrefs.SetString("mode", "level");

                if(PlayerPrefs.GetString("mode") == "level")
                    SpawnBonusStack();
                if(PlayerPrefs.GetString("mode") == "endless")
                {
                    for(int j = 0; j < multiplier + 1; j++)
                    {
                        SpawnBonusStack();
                    }
                }
            }


            yield return null;
            
        }

        bonusStacks = false;
        StartCoroutine(SpawnStack());
    }

    void SpawnBonusStack()
    {

        GameObject stack = stackPool.GetPooledObject();
        lastSpawnedStack = stack;
        stack.transform.position = new Vector3(currentPlatformCenter.x, transform.position.y, currentPlatformCenter.z);

        stack.GetComponent<StackController>().col = ShiftColor();
        stack.SetActive(true);
        stack.GetComponent<StackController>().shouldMove = false;
        stackCount++;
        transform.position += new Vector3(0, stackHeight, 0);
    }

    void CheckAndMakeStacks()
    {

        if ((PlayerPrefs.GetString("mode") == "level" && theLevelController.levelRequirements[theLevelController.level] - theScoreManager.score > 2 * stackHeight) || PlayerPrefs.GetString("mode") == "endless" || !PlayerPrefs.HasKey("mode"))
        {
            if (!PlayerPrefs.HasKey("mode"))
                PlayerPrefs.SetString("mode", "level");

            MakeStack(0);
        }
    }

    public IEnumerator SpawnLevelMarker()
    {
        for(int i = 0; i < theLevelController.levelRequirements.Length; i++)
        {
            GameObject marker = Instantiate(levelMarker, new Vector3(0, theLevelController.levelRequirements[i]/2 + 0.25f, 0) + levelMarkerOffset, Quaternion.identity);
            levelMarkers[i] = marker;
            yield return null;
        }
    }

    public IEnumerator DestroyLevelMarkers()
    {
        for(int i = 0; i < levelMarkers.Length; i++)
        {
            Destroy(levelMarkers[i]);

            yield return null;
        }
    }

   
    public void MakeStack(float offset)
    {
        theLevelController.canCelebrate = true;

        GameObject stack = stackPool.GetPooledObject();
        lastSpawnedStack = stack;
        if (stackCount % 2 == 0)
        {
            stackOffset = currentStackWidth + constantOffset;
            stack.transform.position = new Vector3(transform.position.x - stackOffset, transform.position.y - offset, currentPlatformCenter.z);
            stack.GetComponent<StackController>().comingFromX = true;
        }
        else
        {
            stackOffset = currentStackLength + constantOffset;
            stack.transform.position = new Vector3(currentPlatformCenter.x, transform.position.y - offset, transform.position.z - stackOffset);
            stack.GetComponent<StackController>().comingFromX = false;
        }

        stack.GetComponent<StackController>().col = ShiftColor();
        stack.SetActive(true);
        stack.GetComponent<StackController>().shouldMove = true;
    }

    IEnumerator Dance()
    {
        yield return new WaitForSeconds(danceTime);

        danceInitiated = false;
        danceTime = Random.Range(danceTimeMin, danceTimeMax);

        davesAnim.SetInteger("DanceSelector", Random.Range(1, 7));
    }

    private void LateUpdate()
    {
        davesAnim.SetBool("Restart", false);
        davesAnim.SetInteger("DanceSelector", 0);
        davesAnim.SetBool("BeatGame", false);


    }

    IEnumerator SetDeadzone()
    {
        for (int i = 0; i < 100; i++)
            yield return null;

    }

    public void RestartGame()
    {
        theUIController.gamePlayOverlay.SetActive(true);
        theUIController.ResetPowerup();
        UIController.increasePower = false;

        theLeaderboard.canUnBlur = true;
        theUIController.CloseUsernameSubmission();
        theLeaderboard.canLerpText = true;

        multiplier = 0;
        perfectsInARow = 0;

        if (PlayerPrefs.GetString("mode") == "endless")
        {
            Destroy(theScoreManager.marker);
            theScoreManager.SpawnHighScoreMarker();
        }

        theScoreManager.highScoreMetThisTry = false;
        stackHeight = startingStackHeight;

        theLevelController.openedUmbrella = false;
        theLevelController.closedUmbrella = true;
        theLevelController.davesUmbrella.transform.localScale = Vector3.zero;

        if (theLevelController.openCo != null)
            theLevelController.StopCoroutine(theLevelController.openCo);
        if (theLevelController.closeCo != null)
            theLevelController.StopCoroutine(theLevelController.closeCo);

        theAudioManager.scream.Stop();
        theAudioManager.thud.Stop();

        var composer = theLevelController.vcam.GetCinemachineComponent<CinemachineComposer>();
        composer.m_DeadZoneWidth = 0f;
        composer.m_DeadZoneHeight = 0f;


        theLevelController.vcam.LookAt = FindObjectOfType<StackSpawner>().topStack.transform;

       // StartCoroutine(SetDeadzone());
        theUIController.pauseButton.SetActive(true);
        theAudioManager.perfectSoundIndex = 0;
        theUIController.HideLevelCompletionScreen();

        for (int i = 0; i < davesRigidbodies.Length; i++)
        {
            davesRigidbodies[i].isKinematic = true;
        }

        topStack.GetComponent<TopStackController>().hasInitiatedFall = false;

        davesAnim = dave.GetComponent<Animator>();
        davesAnim.enabled = true;
        teetering = false;
        stackOffset = startingStackOffset;
        stackCount = 0;
        restartButton.SetActive(false);
        // transform.position = restartPos;
        if(theLevelController.level > 0)
        {
            transform.position = new Vector3(0, (theLevelController.levelRequirements[theLevelController.level - 1] + 0.25f) / 2f, 0);
        }
        else
        {
            transform.position = new Vector3(0, 0.125f, 0);
        }
        
        theLevelController.inGame = true;
        currentStackLength = startingStackWidth;

        PlatformDestroyer[] objectsToDeactivate = FindObjectsOfType<PlatformDestroyer>();

        for(int i = 0; i < objectsToDeactivate.Length; i++)
        {
            objectsToDeactivate[i].gameObject.SetActive(false);
        }

        currentStackWidth = startingStackWidth;
        stackSpawned = false;
        currentPlatformCenter = transform.position;
        gameOver = false;

        davesAnim.SetBool("Nervous", false);
        davesAnim.SetBool("TeeterSmall", false);
        davesAnim.SetBool("TeeterLarge", false);
        davesAnim.SetBool("GameOver", false);
        davesAnim.SetBool("Restart", true);
        davesAnim.SetBool("Victory", false);
    }



    Color ShiftColor()
    {
        int order = Random.Range(0, 3);


        if (new Vector3(r, g, b).magnitude <= colorMinTolerance)
        {
            colorStep = new Vector3(Mathf.Abs(colorStep.x), Mathf.Abs(colorStep.y), Mathf.Abs(colorStep.z));
        }

        if (new Vector3(r, g, b).magnitude >= colorMaxTolerance)
        {
            colorStep = new Vector3(-Mathf.Abs(colorStep.x), -Mathf.Abs(colorStep.y), -Mathf.Abs(colorStep.z));
        }

        if (r <= colParamMinValue)
            r = colParamMinValue;
        if (g <= colParamMinValue)
            g = colParamMinValue;
        if (b <= colParamMinValue)
            b = colParamMinValue;


        switch (order)
        {
            case 0:
                r += colorStep.x;
                if (r > 255 || r < 0)
                {
                    colorStep = new Vector3(-colorStep.x, colorStep.y, colorStep.z);
                    g += colorStep.y;
                }
                if (g > 255 || g < 0)
                {
                    colorStep = new Vector3(colorStep.x, -colorStep.y, colorStep.z);
                    b += colorStep.z;
                }
                if (b > 255 || g < 0)
                {
                    colorStep = new Vector3(colorStep.x, colorStep.y, -colorStep.z);
                }
                break;

            case 1:
                g += colorStep.y;
                if (g > 255 || g < 0)
                {
                    colorStep = new Vector3(colorStep.x, -colorStep.y, colorStep.z);
                    r += colorStep.x;
                }
                if (r > 255 || r < 0)
                {
                    colorStep = new Vector3(-colorStep.x, colorStep.y, colorStep.z);
                    b += colorStep.z;
                }
                if (b > 255 || b < 0)
                {
                    colorStep = new Vector3(colorStep.x, colorStep.y, -colorStep.z);
                }
                break;

            case 2:
                b += colorStep.z;
                if (b > 255 || b < 0)
                {
                    colorStep = new Vector3(colorStep.x, colorStep.y, -colorStep.z);
                    g += colorStep.y;
                }
                if (g > 255 || g < 0)
                {
                    colorStep = new Vector3(colorStep.x, -colorStep.y, colorStep.z);
                    r += colorStep.x;
                }
                if (r > 255 || r < 0)
                {
                    colorStep = new Vector3(-colorStep.x, colorStep.y, colorStep.z);
                }
                break;
        }



        compColor = CompColor();
        invColor = InvColor();

        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1);

    }

    Color CompColor()
    {
        //Determine the complementary color for the top stack
        float R = Mathf.Max(r, g, b) + Mathf.Min(r, g, b) - r;
        float G = Mathf.Max(r, g, b) + Mathf.Min(r, g, b) - g;
        float B = Mathf.Max(r, g, b) + Mathf.Min(r, g, b) - b;

        return new Color(R / 255.0f, G / 255.0f, B / 255.0f, 1);
    }

    Color InvColor()
    {
        float R = 255 - r;
        float G = 255 - g;
        float B = 255 - b;

        return new Color(R / 255.0f, G / 255.0f, B / 255.0f, 1);
    }

}
