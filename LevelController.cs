using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Michsky.UI.ModernUIPack;
using UnityEngine.Rendering.PostProcessing;

public class LevelController : MonoBehaviour
{


    public Renderer atmosphereRenderer;
    public SliderManager progressSlider;

    private UIController theUIController;
    private ScoreManager theScoreManager;
    private StackSpawner theStackSpawner;
    private ShipController theShipController;

    public GameObject ship, mainCamera, secondCamera;

    public int level;
    public int[] levelRequirements;
    public bool inGame;
    public bool canCelebrate, sawStartScreen;
    Animator[] starAnim;

    public bool beatenGame;

    public Material sky;
    public Gradient skyCol, eqCol, grColor;
    public float skyLerpSpeed, heightForStars, heightForMaxStars, maxStarSize,
        startChangeToNightHeight, fullNightHeight;

    float skyReferenceHeight;

    public GameObject cutSceneObj, cutsceneSpawnPoint;

    public CinemachineVirtualCamera vcam;

    public bool inCutscene;

    private AudioManager theAudioManager;

    public ColorGrading cg;
    public Vignette vig;
    public Bloom bloom;
    public DepthOfField dof;

    public float spaceTransitionBottom, spaceTransitionTop, maxSat, maxCont, cloudHeightMin, cloudHeightMax, heightToAcheiveMinCloudHeight;

    public int[] levelRainProbabilty;
    public float minTimeBeforeRain, maxTimeBeforeRain, minStormDuration, maxStormDuration, precipWaitMin, precipWaitMax, maxStormLevel;
    public bool stormActive;
    public ParticleSystem rain;
    Coroutine precipCo;
    public Color rainSkyCol;
    public Light sun;
    public float sunIntensityNoRain, sunIntensityRain, rainMultiplier, rainVignettIntensity, rainingUmbrellaScale, openUmbrellaSpeed, openUmbrellaDelay;
    public Camera mainCam;
    public GameObject davesUmbrella, umbrellaFollow;
    public bool openedUmbrella, closedUmbrella;
    public int noStorms;
    public Coroutine openCo, closeCo;

    public float timeBetweenStrikesMin, timeBetweenStrikesMax, lightningDuration;
    float timeBetweenStrikes;
    public GameObject lightning;
    Coroutine lightningCo;
    public GameObject[] strikeZones;
    public float verticalStrikeWindow, lightningVerticalOffset, strikeBloomVal, bloomFadeSpeed, frightLength;
    public GameObject fire;
    public float maxRainHeight;


    public float minSnowIntensity, maxSnowIntensity, snowMinHeight, snowMaxHeight;
    public ParticleSystem snow;
    public bool snowing;
    public ParticleSystemForceField wind;
    public float minGustDuration, maxGustDuration, minGustMagnitude, maxGustMagnitude, equalizerMultiplier;
    float gustSpeed;



    // Start is called before the first frame update
    void Start()
    {
        gustSpeed = Random.Range(minGustDuration, maxGustDuration);
        StartCoroutine(SetGustIntensity());
        snowing = false;
        SetSnowIntensity(0);
        strikeZones = GameObject.FindGameObjectsWithTag("strikeZone");
        openedUmbrella = false;
        closedUmbrella = true;
        SetRainMultiplier(0);

        precipCo = StartCoroutine(PrecipitationLoop());

        PostProcessVolume volume = secondCamera.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out cg);
        PostProcessVolume volumeMain = mainCam.GetComponent<PostProcessVolume>();
        volumeMain.profile.TryGetSettings(out vig);
        volumeMain.profile.TryGetSettings(out bloom);
        volumeMain.profile.TryGetSettings(out dof);
        inCutscene = false;
        theUIController = FindObjectOfType<UIController>();
        theAudioManager = FindObjectOfType<AudioManager>();

        //Show cutscene
        if (!PlayerPrefs.HasKey("showCutscene") || PlayerPrefs.GetInt("showCutscene") == 1)
        {
            inCutscene = true;
            GameObject cutScene = Instantiate(cutSceneObj, cutsceneSpawnPoint.transform.position, cutsceneSpawnPoint.transform.rotation);
            cutScene.transform.localScale = cutsceneSpawnPoint.transform.localScale;
            theUIController.HideStartScreen();
            theAudioManager.StartCoroutine(theAudioManager.FadeInSound(theAudioManager.cutsceneMusic, 1, 1));
        }

        else
        {
            theUIController.ShowStartScreen();
        }


        starAnim = new Animator[3];


        sawStartScreen = false;
        theStackSpawner = FindObjectOfType<StackSpawner>();
        theScoreManager = FindObjectOfType<ScoreManager>();
        theShipController = FindObjectOfType<ShipController>();
        inGame = false;
        canCelebrate = true;

        if(!PlayerPrefs.HasKey("lastBeatenLevel"))
        {
            PlayerPrefs.SetInt("lastBeatenLevel", 0);
        }


        for (int i = 0; i < 3; i++)
        {
            starAnim[i] = theUIController.starImages[i].GetComponent<Animator>();
        }


        SetSkyColor(skyCol.Evaluate(0));
        SetEquatorColor(eqCol.Evaluate(0));
        SetGroundColor(grColor.Evaluate(0));

        //PlayerPrefs.DeleteAll();
    }

    IEnumerator SetGustIntensity()
    {
        float waitTime = Random.Range(minGustDuration, maxGustDuration);

        yield return new WaitForSeconds(waitTime);

        gustSpeed = Random.Range(minGustDuration, maxGustDuration)*theStackSpawner.transform.position.y * equalizerMultiplier;
        wind.directionX = gustSpeed;

        StartCoroutine(SetGustIntensity());
    }

    void SetSnowIntensity(float f)
    {
        var snowEmssion = snow.emission;
        snowEmssion.rateOverTime = f;
    }

    void SetRainMultiplier(float f)
    {
        var rainEmssion = rain.emission;
        rainEmssion.rateOverTime = f;
    }

    void SetSpaceMood()
    {
        if (mainCamera.transform.position.y >= spaceTransitionBottom && mainCamera.transform.position.y < spaceTransitionTop)
        {
            float normalizedCameraPos = (mainCamera.transform.position.y - spaceTransitionBottom) / (spaceTransitionTop - spaceTransitionBottom);

            cg.saturation.value = normalizedCameraPos * maxSat;
            cg.contrast.value = normalizedCameraPos * maxCont;
            SetAtmosphereVisability(normalizedCameraPos);
        }
        else if(mainCamera.transform.position.y >= spaceTransitionTop)
        {
            SetAtmosphereVisability(1);
            cg.saturation.value = Mathf.Lerp(cg.saturation.value, maxSat, Time.deltaTime);
            cg.contrast.value = Mathf.Lerp(cg.contrast.value, maxCont, Time.deltaTime);
        }
        else if(mainCamera.transform.position.y < spaceTransitionBottom)
        {
            SetAtmosphereVisability(0);
            cg.saturation.value = Mathf.Lerp(cg.saturation.value, 0f, Time.deltaTime);
            cg.contrast.value = Mathf.Lerp(cg.contrast.value, 0f, Time.deltaTime);
        }
    }

    void SetAtmosphereVisability(float mult)
    {
        atmosphereRenderer.material.SetFloat("_visability", mult);
    }

    void SetSkyColor(Color col)
    {
        sky.SetColor("_SkyColor", col);
    }
    
    void SetEquatorColor(Color col)
    {
        sky.SetColor("_EquatorColor", col);
    }

    void SetGroundColor(Color col)
    {
        sky.SetColor("_GroundColor", col);
    }

    void LerpSkyColor(Color col)
    {
        sky.SetColor("_SkyColor", Color.Lerp(sky.GetColor("_SkyColor"), col, skyLerpSpeed * Time.deltaTime));
    }

    void LerpEquatorColor(Color col)
    {
        sky.SetColor("_EquatorColor", Color.Lerp(sky.GetColor("_EquatorColor"), col, skyLerpSpeed * Time.deltaTime));
    }

    void LerpGroundColor(Color col)
    {
        sky.SetColor("_GroundColor", Color.Lerp(sky.GetColor("_GroundColor"), col, skyLerpSpeed * Time.deltaTime));
    }

    void SetCloudHeight()
    {
        sky.SetFloat("_CloudsHeight", (mainCamera.transform.position.y / heightToAcheiveMinCloudHeight) * (cloudHeightMin - cloudHeightMax));
    }

    void SetStars()
    {
        float size, s = maxStarSize * (theStackSpawner.transform.position.y - heightForStars) / (heightForMaxStars - heightForStars);

        if (skyReferenceHeight >= heightForStars + 5 && skyReferenceHeight < heightForMaxStars && s > 0)
            size = s;
        else if (skyReferenceHeight < heightForStars + 5)
            size = 0f;
        else
            size = maxStarSize;
        
        sky.SetFloat("_StarsSize", Mathf.Lerp(sky.GetFloat("_StarsSize"), size, skyLerpSpeed * Time.deltaTime));
    }

    void SetSkyboxColor()
    {
        Color sk, eq, gr;
        float skyMultiplier;

        if (skyReferenceHeight >= startChangeToNightHeight && skyReferenceHeight < fullNightHeight)
            skyMultiplier = (theStackSpawner.transform.position.y - startChangeToNightHeight) / (fullNightHeight - startChangeToNightHeight);
        else if (skyReferenceHeight >= fullNightHeight)
            skyMultiplier = 1f;
        else
            skyMultiplier = 0f;

        if (!stormActive)
            sk = skyCol.Evaluate(skyMultiplier);
        else
            sk = rainSkyCol;
        eq = eqCol.Evaluate(skyMultiplier);
        gr = grColor.Evaluate(skyMultiplier);

        LerpSkyColor(sk);
        LerpEquatorColor(eq);
        LerpGroundColor(gr);
    }

    void UpdateSky()
    {
        skyReferenceHeight = mainCamera.transform.position.y;

        SetStars();
        SetSkyboxColor();
    }

    // Update is called once per frame
    void Update()
    {
        

        //in snow range
        if (theStackSpawner.topStack.transform.position.y >= snowMinHeight && theStackSpawner.topStack.transform.position.y <= snowMaxHeight)
        {

            if (!snowing)
            {
                SetSnowIntensity(Random.Range(minSnowIntensity, maxSnowIntensity));
                snowing = true;
            }

        }
        //below snow range
        if (snowing && theStackSpawner.topStack.transform.position.y < snowMinHeight)
        {
            SetSnowIntensity(0);
            snowing = false;
        }
        //above snow range
        if (snowing && theStackSpawner.topStack.transform.position.y > snowMaxHeight)
        {
            SetSnowIntensity(0);
            snowing = false;
        }


        if ((theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("PullOutUmbrella") || theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("UmbrellaIdle")) && !openedUmbrella)
        {
            openCo = StartCoroutine(OpenUmbrella());
        }
        if(theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("PutAwayUmbrella") && !closedUmbrella)
        {
            StartCoroutine(CloseUmbrella());
        }

            UpdateSunIntensity();
        SetCloudHeight();
        SetSpaceMood();
        UpdateSky();

        //Set progress slider
        float sliderVal = 2*theStackSpawner.davesRagdoll.transform.position.y/ levelRequirements[levelRequirements.Length - 1];

        if (sliderVal < 0)
            sliderVal = 0;
        else if (sliderVal > levelRequirements[levelRequirements.Length - 1])
            sliderVal = levelRequirements[levelRequirements.Length - 1];

        progressSlider.mainSlider.value = sliderVal;


        if(!inGame && Input.GetMouseButtonDown(0) && !sawStartScreen && !theUIController.paused && !inCutscene)
        {
            //theUIController.titleImageAnimator.SetBool("inGame", true);
            theUIController.titleImage.SetActive(false);
            if (PlayerPrefs.GetInt("lastBeatenLevel") > 0)
            {
                theUIController.OpenMainMenu();
                sawStartScreen = true;

                theUIController.startScreen.SetActive(false);
            }

            else
            {
                sawStartScreen = true;
                inGame = true;
                theUIController.MMtoGame();
            }

        }

        if(inGame)
        {
            CheckForLevelCompletion();
        }
    }

    private void LateUpdate()
    {

        for(int i = 0; i < 3; i++)
        {
            starAnim[i].SetBool("ReAnimate", false);
        }
        
        theUIController.winScreenBuffer = true;
    }

    void CheckForLevelCompletion()
    {
        if(PlayerPrefs.GetString("mode") == "level" && level < levelRequirements.Length && theScoreManager.score >= levelRequirements[level] && canCelebrate && theUIController.winScreenBuffer)
        {
            //level complete
            theAudioManager.PlayeLevelComplete();
            theStackSpawner.currentStackLength = theStackSpawner.startingStackWidth;
            theStackSpawner.currentStackWidth = theStackSpawner.startingStackWidth;
            theStackSpawner.currentPlatformCenter = new Vector3(0, theStackSpawner.currentPlatformCenter.y, 0);
            
            if(theScoreManager.CheckForStars() > theScoreManager.stars[level])
                theScoreManager.stars[level] = theScoreManager.CheckForStars();

            PlayerPrefsX.SetIntArray("stars", theScoreManager.stars);

            //Animate stars
            for(int i = 0; i < 3; i++)
            {
                if (i < theScoreManager.CheckForStars())
                {
                    starAnim[i].SetBool("Animate", true);
                    starAnim[i].SetBool("ReAnimate", true);
                }
                else
                {
                    starAnim[i].SetBool("Animate", false);
                    starAnim[i].SetBool("ReAnimate", false);
                }
            }

            inGame = false;
            theUIController.ShowLevelCompletionScreen();
            theStackSpawner.davesAnim.SetBool("Victory", true);
            theStackSpawner.davesAnim.SetBool("StartGame", false);
            canCelebrate = false;

            if(PlayerPrefs.HasKey("lastBeatenLevel") && PlayerPrefs.GetInt("lastBeatenLevel") < level + 1)
            {
                PlayerPrefs.SetInt("lastBeatenLevel", level + 1);
            }
           
        }
    }

    public void StartNextLevel()
    {
        if(theAudioManager.levelComplete.isPlaying)
        {
            theAudioManager.StartCoroutine(theAudioManager.FadeOutSound(theAudioManager.levelComplete, 1f));
        }

        if (level < levelRequirements.Length - 1)
        {
            theStackSpawner.lastSpawnedStack.transform.position = new Vector3(0, theStackSpawner.lastSpawnedStack.transform.position.y, 0);
            theStackSpawner.lastSpawnedStack.transform.localScale = new Vector3(theStackSpawner.startingStackWidth, theStackSpawner.lastSpawnedStack.transform.localScale.y, theStackSpawner.startingStackWidth);
            theUIController.HideLevelCompletionScreen();
            theStackSpawner.currentPlatformCenter = new Vector3(0, theStackSpawner.currentPlatformCenter.z, 0);
            theStackSpawner.currentStackWidth = theStackSpawner.startingStackWidth;
            theStackSpawner.currentStackLength = theStackSpawner.startingStackWidth;
            theStackSpawner.davesAnim.SetBool("StartGame", true);
            theStackSpawner.davesAnim.SetBool("Victory", false);
            theStackSpawner.davesAnim.SetBool("Nervous", false);
            theStackSpawner.davesAnim.SetBool("TeeterSmall", false);
            theStackSpawner.davesAnim.SetBool("TeeterLarge", false);
            theStackSpawner.davesAnim.SetInteger("DanceSelector", 0);
            theStackSpawner.MakeStack(theStackSpawner.stackHeight);

            level++;
            inGame = true;
        }

        else
        {
            //Game beaten
            beatenGame = true;
            ShowEndGameSequence();
        }
    }

    public void OpenLevel(int levelz)
    {

        if (levelz >= noStorms)
            StopStorm();

        if(levelz > 1 && levelz < levelRequirements.Length)
        {
            theStackSpawner.transform.position = new Vector3(0, (levelRequirements[levelz - 1] + 0.25f)/2f, 0);
        }

        if(levelz >= levelRequirements.Length)
        {
            level = levelRequirements.Length - 2;
            theStackSpawner.transform.position = new Vector3(0, (levelRequirements[levelz - 2] + 0.25f) / 2f, 0);
        }
            

    }

    void ShowEndGameSequence()
    {
        theUIController.ShowEndGameMenu();
        theStackSpawner.davesAnim.SetBool("BeatGame", true);
        StartCoroutine(DaveWait());
    }

    IEnumerator DaveWait()
    {
        yield return new WaitForSeconds(theStackSpawner.daveWaitToBoard);
        ship.GetComponent<Animator>().SetBool("Board", true);
        theStackSpawner.dave.SetActive(false);

        StartCoroutine(ShipWait());
    }

    IEnumerator ShipWait()
    {
        yield return new WaitForSeconds(theStackSpawner.shipWait);
        ship.GetComponent<ShipController>().flyAway = true;
    }

    void LerpToNight()
    {
        
    }

    void UpdateSunIntensity()
    {
        if (stormActive)
        {
            sun.intensity = Mathf.Lerp(sun.intensity, sunIntensityRain, 2f * Time.deltaTime);
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, rainVignettIntensity, Time.deltaTime);

        }
        else
        {
            sun.intensity = Mathf.Lerp(sun.intensity, sunIntensityNoRain, 3f * Time.deltaTime);
            vig.intensity.value = Mathf.Lerp(vig.intensity.value, 0f, 2f*Time.deltaTime);
        }

        if (!theStackSpawner.gameOver)
        {
            davesUmbrella.transform.position = umbrellaFollow.transform.position;
            davesUmbrella.transform.rotation = umbrellaFollow.transform.rotation;
        }
    }

    void MaybeSpawnStorm()
    {
        if(Random.Range(0f,100f) <= levelRainProbabilty[level] && !stormActive && theStackSpawner.transform.position.y <= maxRainHeight)
        {
            stormActive = true;
            StartCoroutine(SpawnStorm());
        }
    }

    IEnumerator PrecipitationLoop()
    {
        float wait = Random.Range(precipWaitMin, precipWaitMax);

        while (true)
        {
            while (!stormActive)
            {
                yield return new WaitForSeconds(wait);

                MaybeSpawnStorm();

                wait = Random.Range(precipWaitMin, precipWaitMax);
            }

            if(stormActive)
                wait = Random.Range(precipWaitMin, precipWaitMax);

            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator SpawnStorm()
    {
        float timeToWait = Random.Range(minTimeBeforeRain, maxTimeBeforeRain);

        yield return new WaitForSeconds(timeToWait);

        if (lightningCo != null)
            StopCoroutine(lightningCo);

        theAudioManager.StartCoroutine(theAudioManager.FadeInSound(theAudioManager.rain, 1f, 1f));
        SetRainMultiplier(rainMultiplier);
        theStackSpawner.davesAnim.SetBool("Raining", true);

        lightningCo = StartCoroutine(LightningStrike());


        timeToWait = Random.Range(minStormDuration, maxStormDuration);

        yield return new WaitForSeconds(timeToWait);

        StopStorm();
    }

    IEnumerator LightningStrike()
    {
        timeBetweenStrikes = Random.Range(timeBetweenStrikesMin, timeBetweenStrikesMax);



        yield return new WaitForSeconds(timeBetweenStrikes);
        int viableStrikeNumber = 0;

        //Determine strike location
        List<GameObject> viableStrikes = new List<GameObject>();

        for(int i = 0; i < strikeZones.Length; i++)
        {
            if(Mathf.Abs(strikeZones[i].transform.position.y - theStackSpawner.topStack.transform.position.y) <= verticalStrikeWindow)
            {
                viableStrikes.Add(strikeZones[i]);
                viableStrikeNumber++;
            }
        }

        if (viableStrikeNumber > 0)
        {
            GameObject strikeLocation = viableStrikes[Random.Range(0, viableStrikes.Count)];
            Vector3 strikeBounds = strikeLocation.GetComponent<BoxCollider>().bounds.size;

            lightning.transform.position = strikeLocation.transform.position + new Vector3(Random.Range(-strikeBounds.x / 2, strikeBounds.x / 2), lightningVerticalOffset, Random.Range(-strikeBounds.z / 2, strikeBounds.z / 2));

            if(Vector3.Distance(strikeLocation.transform.position, theStackSpawner.topStack.transform.position) < 2f && theStackSpawner.davesAnim.GetCurrentAnimatorStateInfo(0).IsName("UmbrellaIdle"))
            {
                theStackSpawner.davesAnim.SetBool("Scared", true);
                StartCoroutine(StopBeingScared());
            }

            lightning.SetActive(true);
            theAudioManager.lightning.Stop();
            theAudioManager.lightning.Play();
            theAudioManager.lightning.volume = theAudioManager.lightningVolume * theAudioManager.globalEffectVolume;
            bloom.intensity.value = strikeBloomVal;
            GameObject lightningFire = Instantiate(fire, lightning.transform.position, Quaternion.Euler(-90f,0f,0f));
            lightningFire.GetComponent<FireController>().trackObj = strikeLocation;

            yield return new WaitForSeconds(lightningDuration);
            lightning.transform.position = new Vector3(1000000, 1000000, 1000000);

            yield return new WaitForSeconds(0.02f);
            lightning.SetActive(false);

            while(bloom.intensity.value > 0f)
            {
                bloom.intensity.value -= bloomFadeSpeed * Time.deltaTime;
                yield return null;
            }
            bloom.intensity.value = 0f;
        }

        lightningCo = StartCoroutine(LightningStrike());
    }

    IEnumerator StopBeingScared()
    {
        yield return new WaitForSeconds(frightLength);

        theStackSpawner.davesAnim.SetBool("Scared", false);
    }

    public void StopStorm()
    {
        if (lightningCo != null)
            StopCoroutine(lightningCo);

        theAudioManager.StartCoroutine(theAudioManager.FadeOutSound(theAudioManager.rain, 1f));
        closeCo = StartCoroutine(CloseUmbrella());
        SetRainMultiplier(0f);
        theStackSpawner.davesAnim.SetBool("Raining", false);
        stormActive = false;
    }

    IEnumerator OpenUmbrella()
    {

        if (closeCo != null)
            StopCoroutine(closeCo);
        openedUmbrella = true;
        closedUmbrella = false;
        yield return new WaitForSeconds(openUmbrellaDelay);

        bool canOpenUmbrella = true; // false;

        if (davesUmbrella.GetComponent<Rigidbody>() != null)
            Destroy(davesUmbrella.GetComponent<Rigidbody>());



        if(canOpenUmbrella)
        {
            //davesUmbrella.SetActive(true);

            while (davesUmbrella.transform.localScale.x < rainingUmbrellaScale)
            {
                davesUmbrella.transform.localScale += Vector3.one * openUmbrellaSpeed * Time.deltaTime;
                yield return null;
            }
        }

    }

    IEnumerator CloseUmbrella()
    {

        if (openCo != null)
            StopCoroutine(openCo);

        closedUmbrella = true;
        if (!theStackSpawner.gameOver)
        {
            
            while (davesUmbrella.transform.localScale.x > 0)
            {
                davesUmbrella.transform.localScale -= Vector3.one * openUmbrellaSpeed * Time.deltaTime;
                yield return null;
            }

            davesUmbrella.transform.localScale = Vector3.zero;

            openedUmbrella = false;
        }
    }
}
