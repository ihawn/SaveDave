using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using UnityEngine.Diagnostics;

public class UIController : MonoBehaviour
{
    public Vector3 normalFollowOffset, cutSceneFollowOffset;

    public Text debugText;
    public Button[] levelButtons;
    private ScoreManager theScoreManager;
    private StackSpawner theStackSpawner;
    private LevelController theLevelController;
    private AudioManager theAudioManager;
    private QualityController theQualityController;
    public GameObject titleImage, startScreen, gamePlayOverlay;
    public Animator titleImageAnimator;
    public GameObject levelCompletionScreen, deathScreen, mainMenu, levelMenu, endGameMenu, pauseMenu, pauseButton;
    Animator lcAnim;
    public bool wasInDeathMenu;
    public bool winScreenBuffer;

    public Image[] starImages;
    public Image[] levelStarImages;

    public Sprite emptyStar, fullStar, lockSprite;

    public Text postCutsceneText, feedbackText;
    public Color badCol, medCol, goodCol;
    public string[] badText, medText, goodText;

    public GameObject CutSceneUI;

    public bool paused;

    public HorizontalSelector resSelector, qualSelector, waterfallQualSelector;
    public string[] qualityNames, waterfallQualityNames;

    public Text fpsCounter;

    public float feedbackFadeSpeed;
    Coroutine co;

    public Text multiplierText;

    private void Awake()
    {
     //  DeleteAndCrash();
    }

    // Start is called before the first frame update
    void Start()
    {

        co = StartCoroutine(FadeFeedback());
        feedbackText.gameObject.SetActive(false);

        theAudioManager = FindObjectOfType<AudioManager>();
        winScreenBuffer = true;
        debugText.text = "";

        theQualityController = FindObjectOfType<QualityController>();
        theScoreManager = FindObjectOfType<ScoreManager>();
        lcAnim = levelCompletionScreen.GetComponent<Animator>();
        theStackSpawner = FindObjectOfType<StackSpawner>();
        theLevelController = FindObjectOfType<LevelController>();

        titleImageAnimator = titleImage.GetComponent<Animator>();

        titleImageAnimator.SetBool("inGame", false);

        CutSceneUI.SetActive(false);

        // StartCoroutine(InitSettings());

        //set resolution options
        if (resSelector.itemList.Count == 0)
        {
            theQualityController.resolutions = new Vector2[6];

            for (int i = 6; i > 0; i--)
            {
                theQualityController.resolutions[i - 1] = new Vector2(3 * Display.main.systemWidth / (i + 2), 3 * Display.main.systemHeight / (i + 2));
                resSelector.CreateNewItem("" + (int)theQualityController.resolutions[i - 1].x + " X " + (int)theQualityController.resolutions[i - 1].y);

            }
        }
        //set quality options
        if (qualSelector.itemList.Count == 0)
        {
            for (int i = 0; i < qualityNames.Length; i++)
            {
                qualSelector.CreateNewItem(qualityNames[i]);

            }
        }
        //set waterfall options
        if (waterfallQualSelector.itemList.Count== 0)
        {
            for(int i = 0; i < waterfallQualityNames.Length; i++)
            {
                waterfallQualSelector.CreateNewItem(waterfallQualityNames[i]);
            }
        }

        if(PlayerPrefs.HasKey("res"))
            theQualityController.SetResolution(PlayerPrefs.GetInt("res"));
        if(PlayerPrefs.HasKey("qual"))
            theQualityController.SetQuality(PlayerPrefs.GetInt("qual"));
        else
            QualitySettings.SetQualityLevel(5, true);
        if (PlayerPrefs.HasKey("waterfallQual"))
            theQualityController.SetWaterfallQuality(PlayerPrefs.GetInt("waterfallQual"));
        else
        {
            PlayerPrefs.SetInt("waterfallQual", 2);
            theQualityController.SetWaterfallQuality(2);
        }

        multiplierText.text = "";
    }

    private void Update()
    {
        fpsCounter.text = "" + 1f / Time.deltaTime;

    }

    IEnumerator FadeFeedback()
    {
        Color col = feedbackText.color;
        float alpha = col.a;

        while(alpha > 0)
        {
            feedbackText.color = new Color(col.r, col.g, col.b, alpha);
            alpha -= Time.deltaTime*feedbackFadeSpeed;
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }

    public void ShowFeedbackText(string whichOne)
    {
        feedbackText.gameObject.SetActive(true);
        StopCoroutine(co);

        switch(whichOne)
        {
            case "bad":
                feedbackText.color = badCol;
                ShowBadText();
                break;

            case "medium":
                feedbackText.color = medCol;
                ShowMediumText();
                break;

            case "good":
                feedbackText.color = goodCol;
                ShowGoodText();
                break;

            case "bonusGood":
                feedbackText.color = goodCol;
                ShowGoodBonusText();
                break;

            case "bonusBad":
                feedbackText.color = badCol;
                ShowBadBonusText();
                break;
        }

        co = StartCoroutine(FadeFeedback());
    }

    void ShowBadText()
    {
        feedbackText.text = badText[Random.Range(0, badText.Length)];
    }

    void ShowMediumText()
    {
        feedbackText.text = medText[Random.Range(0, medText.Length)];
    }

    void ShowGoodText()
    {
        feedbackText.text = goodText[Random.Range(0, goodText.Length)];
    }

    void ShowGoodBonusText()
    {
        feedbackText.text = "Bonus!";
    }

    void ShowBadBonusText()
    {
        feedbackText.text = "miss";
    }

    public void UnlockAll()
    {
        PlayerPrefs.SetInt("lastBeatenLevel", theLevelController.levelRequirements.Length - 1);
    }

    //dont run this ever unless you are purposly trying to crash system
    public void DeleteAndCrash()
    {
        PlayerPrefs.DeleteAll();
        Utils.ForceCrash(ForcedCrashCategory.Abort);
    }

    IEnumerator InitSettings()
    {
        yield return new WaitForEndOfFrame();

        InitializeSettingsMenu();
    }


    void InitializeSettingsMenu()
    {

        resSelector.defaultIndex = 5 - PlayerPrefs.GetInt("res");
        resSelector.selectorEvent.AddListener(delegate { theQualityController.SetResolution(theQualityController.resolutions.Length - resSelector.index - 1); });

        qualSelector.defaultIndex = PlayerPrefs.GetInt("qual");
        qualSelector.selectorEvent.AddListener(delegate { theQualityController.SetQuality(qualSelector.index); });

        waterfallQualSelector.defaultIndex = PlayerPrefs.GetInt("waterfallQual");
        waterfallQualSelector.selectorEvent.AddListener(delegate { theQualityController.SetWaterfallQuality(waterfallQualSelector.index); });
    }

    public void StartEndless()
    {
        PlayerPrefs.SetString("mode", "endless");
        pauseButton.SetActive(true);

        //  debugText.text = "" + PlayerPrefs.GetInt("lastBeatenLevel");

        SelectLevel(1);

        mainMenu.SetActive(false);
       // theScoreManager.SpawnHighScoreMarker();
    }
    

    public void PauseGame()
    {
        InitializeSettingsMenu();
        paused = true;
        gamePlayOverlay.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        paused = false;
        gamePlayOverlay.SetActive(true);
        pauseButton.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ShowCutsceneOnNextSession()
    {
        PlayerPrefs.SetInt("showCutscene", 1);
    }

    public void DontShowCutsceneOnNextSession()
    {
        PlayerPrefs.SetInt("showCutscene", 0);
    }

    public void HideStartScreen()
    {
        titleImage.SetActive(false);
        startScreen.SetActive(false);
    }

    public void ContinueFromCutScene()
    {
        theAudioManager.StartCoroutine(theAudioManager.FadeOutSound(theAudioManager.cutsceneMusic, 0.15f));
        CutSceneUI.SetActive(true);
        postCutsceneText.text = "Dave hopes you liked his drawings";

        StartCoroutine(AfterCutsceneTextProgression());
    }

    IEnumerator AfterCutsceneTextProgression()
    {
        yield return new WaitForSeconds(3.5f);

        postCutsceneText.text = "And that you can save him...";

        StartCoroutine(WaitForButton());
    }

    IEnumerator WaitForButton()
    {
        yield return new WaitForSeconds(2.5f);

        ShowStartScreen();
        theLevelController.inCutscene = false;
    }

    public void ShowStartScreen()
    {
        CutSceneUI.SetActive(false);
        titleImage.SetActive(true);
        startScreen.SetActive(true);
    }

    public void MMtoGame()
    {
        startScreen.SetActive(false);
        gamePlayOverlay.SetActive(true);
        theStackSpawner.davesAnim.SetBool("StartGame", true);
    }

    public void ShowLevelCompletionScreen()
    {
        lcAnim.SetBool("Restart", true);
        lcAnim.SetBool("StartToFade", false);
        pauseButton.SetActive(false);
    }

    public void HideLevelCompletionScreen()
    {
        pauseButton.SetActive(true);
        lcAnim.SetBool("Restart", false);
        lcAnim.SetBool("StartToFade", true);
    }

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void OpenMainMenu()
    {
        // titleImageAnimator.SetBool("inGame", true);
        theStackSpawner.StartCoroutine(theStackSpawner.DestroyLevelMarkers());
        endGameMenu.SetActive(false);
        titleImage.SetActive(false);
        HideLevelCompletionScreen();
        levelMenu.SetActive(false);
        deathScreen.SetActive(false);
        mainMenu.SetActive(true);
        theLevelController.inGame = false;
        wasInDeathMenu = false;
        theStackSpawner.davesAnim.SetBool("Restart", true);
        theStackSpawner.davesAnim.SetBool("StartGame", false);
    }

    public void OpenLevels()
    {
        PlayerPrefs.SetString("mode", "level");
        mainMenu.SetActive(false);
        deathScreen.SetActive(false);
        levelMenu.SetActive(true);
        RefreshUnlockedLevels();
        RefreshStarCount();
    }

    public void CloseLevels()
    {
        if(wasInDeathMenu)
        {
            deathScreen.SetActive(true);
        }
        else
        {
            mainMenu.SetActive(true);
        }
        levelMenu.SetActive(false);
        
    }

    public void Continue()
    {
        pauseButton.SetActive(true);
        PlayerPrefs.SetString("mode", "level");

        //  debugText.text = "" + PlayerPrefs.GetInt("lastBeatenLevel");

        if (PlayerPrefs.GetInt("lastBeatenLevel") < theLevelController.levelRequirements.Length)
            SelectLevel(PlayerPrefs.GetInt("lastBeatenLevel") + 1);
        else
            SelectLevel(PlayerPrefs.GetInt("lastBeatenLevel"));

        mainMenu.SetActive(false);
    }

    public void SelectLevel(int l)
    {
        if(PlayerPrefs.GetString("mode") == "level" || !PlayerPrefs.HasKey("mode"))
            theStackSpawner.StartCoroutine(theStackSpawner.SpawnLevelMarker());

        if (theScoreManager.marker != null)
            Destroy(theScoreManager.marker);

        theAudioManager.PlayLevelWhoosh();

        winScreenBuffer = false;
        levelMenu.SetActive(false);
        theLevelController.inGame = true;
        theLevelController.OpenLevel(l);
        theLevelController.level = l - 1;
        theLevelController.canCelebrate = false;
        theStackSpawner.RestartGame();

        gamePlayOverlay.SetActive(true);

    }

    public void SelectCurrentLevel()
    {
        SelectLevel(theLevelController.level + 1);
    }

    void RefreshUnlockedLevels()
    {
        for(int i = 0; i < levelButtons.Length; i++)
        {
            if(i < PlayerPrefs.GetInt("lastBeatenLevel") + 1)
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }

    void RefreshStarCount()
    {
        for (int i = 0; i < theLevelController.levelRequirements.Length; i++)
        {
            if (levelButtons[i].interactable)
            {
                for (int j = 0; j < 3; j++)
                {
                    levelStarImages[3 * i + j].gameObject.SetActive(true);
                    if (theScoreManager.stars[i] > j)
                    {
                        //filled star
                        levelStarImages[3 * i + j].sprite = fullStar;
                    }
                    else
                    {
                        //empty star
                        levelStarImages[3 * i + j].sprite = emptyStar;
                    }
                }
            }

            else
            {
                levelStarImages[3 * i].gameObject.SetActive(false);
                levelStarImages[3 * i + 1].sprite = lockSprite;
                levelStarImages[3 * i + 2].gameObject.SetActive(false);
            }

        }


    }

    public void ShowEndGameMenu()
    {
        endGameMenu.SetActive(true);
        HideLevelCompletionScreen();
        gamePlayOverlay.SetActive(false);
    }
}
