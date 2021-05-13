using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;


public class Leaderboards : MonoBehaviour
{
    public TextMeshProUGUI usernamePromptText;
    public float textFieldLerpMultiplier, fieldLerpSpeed;
    public bool canLerpText;
    public string username;
    public int minUsernameLen, maxUsernameLen;
    public GameObject usernameField;
    public bool usernameFieldActive;
    public TextMeshProUGUI[] userField, scoreField;
    private UIController theUIController;
    private ScoreManager theScoreManager;
    private TMP_InputField theInputField;
    public int place;

    //View leaderboard database at https://www.dreamlo.com/lb/P5gjtd5i6kOpx-wpb9Mtqwgyqsz12-i02cHh02Uct_xA
    //these are the old ones for reference


    const string privateCode = "P5gjtd5i6kOpx-wpb9Mtqwgyqsz12-i02cHh02Uct_xA";
    const string publicCode = "609ae6788f40c30ca04b4889";
    const string webURL = "http://www.dreamlo.com/lb/";

    public Highscore[] highscoresList;
    public Highscore[] compScoreList;
    public Highscore[] winnersList;


    private void Awake()
    {
        place = 0;
        username = "";

        theScoreManager = FindObjectOfType<ScoreManager>();

        if (theScoreManager.highScore > 1 && PlayerPrefs.HasKey("username"))
            UploadNewHighScore(PlayerPrefs.GetString("username"), (int)Mathf.Round(theScoreManager.highScore));
        DownloadHighScores();

        if (PlayerPrefs.HasKey("username"))
            username = PlayerPrefs.GetString("username");
    }

    private void Start()
    {
        canLerpText = true;
        theInputField = usernameField.GetComponent<TMP_InputField>();
        theInputField.characterLimit = maxUsernameLen;
       // username = "";
        theUIController = FindObjectOfType<UIController>();

        theInputField.onEndEdit.AddListener(delegate { OnUserFieldSubmission(); });
        // DownloadHighScores();

    }

    void Update()
    {
        if(theInputField.isFocused)
        {
            if(canLerpText)
            {
                canLerpText = false;
                StartCoroutine(LerpText());
                theUIController.deathScreen.SetActive(false);
            }
        }
    }

    IEnumerator GetRank()
    {
        if(highscoresList != null  && username.Length >= minUsernameLen && theScoreManager.highScore >= 1)
        {
            for(int i = 0; i < 10; i++)
            {
                if(highscoresList[i].username == username)
                {
                    place = i + 1;
                    break;
                }
                yield return null;
            }
        }
    }

    IEnumerator LerpText()
    {
        Vector3 lerpPos = theInputField.transform.position + new Vector3(0f, Screen.height * textFieldLerpMultiplier, 0f);


        while(Vector3.Distance(theInputField.transform.position, lerpPos) >= 0.001f)
        {
            theInputField.transform.position = Vector3.Lerp(theInputField.transform.position, lerpPos, fieldLerpSpeed * Time.deltaTime);

            yield return null;
        }
    }

    private void OnEnable()
    {

        //   DownloadHighScores();


    }

    string GetUsernameInput()
    {
        return theInputField.text;
    }

    public void OnUserFieldSubmission()
    {
        string userText = GetUsernameInput();

        if (userText.Length >= minUsernameLen)
        {
            userText = FindObjectOfType<Censor>().CensorText(userText);
            usernameFieldActive = false;


            usernameField.gameObject.SetActive(false);
            string usernameStorage = userText;
            
            Truncate(usernameStorage, maxUsernameLen);

            usernameStorage = usernameStorage.Replace(" ", "_");
            PlayerPrefs.SetString("username", usernameStorage);


            int highScore = (int)PlayerPrefs.GetFloat("HighScore");

            username = usernameStorage;
            PlayerPrefs.SetString("username", username);

            theUIController.deathScreen.SetActive(true);

            AddNewHighScore(username, (int) Mathf.Round(theScoreManager.highScore));
        }
    }

    public void AddNewHighScore(string username, int score)
    {
        print("Attempting to add new high score");
        StartCoroutine(UploadNewHighScore(username, score));
    }

    public void DownloadHighScores()
    {
        StartCoroutine(GetHighScores());
    }

    public void CheckUsernameRequirements()
    {
        if (GetUsernameInput().Length < minUsernameLen)
            usernamePromptText.text = "Too short";
        else
            usernamePromptText.text = "";
    }


    public struct Highscore
    {
        public string username;
        public int score;

        public Highscore(string _username, int _score)
        {
            username = _username;
            score = _score;

        }
    }

    void FormatHighscores(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        highscoresList = new Highscore[entries.Length];



        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            int score = int.Parse(entryInfo[1]);
            highscoresList[i] = new Highscore(username, score);

            int place = i + 1;



        }
    }


    public IEnumerator UploadNewHighScore(string username, int score)
    {

        UnityWebRequest www = new UnityWebRequest(webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score);
        yield return www.SendWebRequest();

        if (string.IsNullOrEmpty(www.error))
        {
            print("High score uploaded successfully :)");
        }

        else
        {
            print("Error uploading high score: " + www.error);
        }

        DownloadHighScores();
    }

    IEnumerator GetHighScores()
    {
        print("Retrieving high scores");

        //receive
        UnityWebRequest www = new UnityWebRequest(webURL + publicCode + "/pipe/");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();


        if (string.IsNullOrEmpty(www.error))
        {
            FormatHighscores(www.downloadHandler.text);

            for(int i = 0; i < userField.Length && i < highscoresList.Length; i++)
            {
                userField[i].text = (i + 1).ToString() + ") " + highscoresList[i].username;
                scoreField[i].text = highscoresList[i].score.ToString();
            }
        }

        else
        {
            print("Error downloading high score: " + www.error);
            //   scoreBox.text = "Retrieving Scores...";
        }

        StartCoroutine(GetRank());
    }


    public string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) { return value; }
        return value.Substring(0, Mathf.Min(value.Length, maxLength));
    }

 
 }
