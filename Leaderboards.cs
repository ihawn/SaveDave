using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class HighScores : MonoBehaviour
{
    public bool usernameFieldActive;
    public InputField usernameField;
    public GameObject settingsButton, titleImage, buttonContainer, leaderboard, everythingButWonScreen, wonScreen;

    //View leaderboard database at https://www.dreamlo.com/lb/P5gjtd5i6kOpx-wpb9Mtqwgyqsz12-i02cHh02Uct_xA
    //these are the old ones for reference


    const string privateCode = "P5gjtd5i6kOpx-wpb9Mtqwgyqsz12-i02cHh02Uct_xA";
    const string publicCode = "609ae6788f40c30ca04b4889";
    const string webURL = "http://www.dreamlo.com/lb/";

    public Highscore[] highscoresList;
    public Highscore[] compScoreList;
    public Highscore[] winnersList;

    public Text userBox, scoreBox, compUserBox, compScoreBox, timerText, rewardText;


    private void Start()
    {


// usernameField.onEndEdit.AddListener(delegate { OnUserFieldSubmission(); });
        // DownloadHighScores();

    }

    private void OnEnable()
    {
        if (userBox.text != null)
        {
            userBox.text = "Retrieving Users...";
            scoreBox.text = "Retrieving Scores...";
            compUserBox.text = "Retrieving Users...";
            compScoreBox.text = "Retrieving Scores...";
        }

        //   DownloadHighScores();


    }

    private void Awake()
    {

        DownloadHighScores();
    }


    public void AddNewHighScore(string username, int score)
    {
        StartCoroutine(UploadNewHighScore(username, score));
    }

    public void DownloadHighScores()
    {
        StartCoroutine(GetHighScores());
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

        if (userBox != null)
        {
            userBox.text = "";
            scoreBox.text = "";
        }



        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            int score = int.Parse(entryInfo[1]);
            highscoresList[i] = new Highscore(username, score);

            int place = i + 1;

            if (userBox != null)
            {

                userBox.text += place + ") " + highscoresList[i].username + "\n------------------------------------------\n";
                scoreBox.text += highscoresList[i].score + "\n----------\n";
            }

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


    }

    IEnumerator GetHighScores()
    {
        if (userBox != null)
        {
            userBox.text = "Retrieving Users...";
            scoreBox.text = "Retrieving Scores...";
        }


        //receive
        UnityWebRequest www = new UnityWebRequest(webURL + publicCode + "/pipe/");
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();


        if (string.IsNullOrEmpty(www.error))
        {
            FormatHighscores(www.downloadHandler.text);
        }

        else
        {
            print("Error downloading high score: " + www.error);
            //   scoreBox.text = "Retrieving Scores...";
        }
    }


    public string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) { return value; }
        return value.Substring(0, Mathf.Min(value.Length, maxLength));
    }

 
 }
