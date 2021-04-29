using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private StackSpawner theStackSpawner;
    private LevelController theLevelController;
    public Text scoreText;
    public float score, highScore;

    public float star1Threshold, star2Threshold, star3Threshold;

    public int[] stars;
    public bool highScoreMetThisTry;

    public GameObject highScoreMarker, marker;
    public HighScoreMarkerController markerController;
    private AudioManager theAudioManager;

    // Start is called before the first frame update
    void Start()
    {
        //   PlayerPrefs.DeleteAll();
        theAudioManager = FindObjectOfType<AudioManager>();
        theStackSpawner = FindObjectOfType<StackSpawner>();
        theLevelController = FindObjectOfType<LevelController>();

        if (PlayerPrefsX.GetIntArray("stars") != null && PlayerPrefsX.GetIntArray("stars").Length == stars.Length)
        {
            stars = PlayerPrefsX.GetIntArray("stars");
        }

        if(PlayerPrefs.HasKey("HighScore"))
        {
            highScore = PlayerPrefs.GetFloat("HighScore");
        }
        else
        {
            highScore = 0;
            PlayerPrefs.SetFloat("HighScore", highScore);
        }

    }

    // Update is called once per frame
    void Update()
    {
        score = theStackSpawner.transform.position.y * 2 - 0.25f;
        if(score >= 0)
        {

            if (PlayerPrefs.GetString("mode") == "level")
            {
                if (theLevelController.level < theLevelController.levelRequirements.Length)
                    scoreText.text = "" + score.ToString("F1") + "/" + theLevelController.levelRequirements[theLevelController.level].ToString("F0") + "M";
            }
            else if(PlayerPrefs.GetString("mode") == "endless")
            {
                scoreText.text = "" + score.ToString("F1");

                if (score > highScore)
                {
                    highScore = score;
                    PlayerPrefs.SetFloat("HighScore", highScore);

                    if(!highScoreMetThisTry && markerController != null)
                    {
                        markerController.StartCoroutine(markerController.AddTickPhysics());
                        highScoreMetThisTry = true;
                        theAudioManager.PlayHighScore();
                    }
                }
            }
        }

        //PlayerPrefs.SetFloat("HighScore", 11);

    }

    public void SpawnHighScoreMarker()
    {
        if (highScore > 10f)
        {
            marker = Instantiate(highScoreMarker, new Vector3(0f, (highScore + 0.25f) / 2f, 0f), Quaternion.identity);
            markerController = marker.GetComponent<HighScoreMarkerController>();
        }
    }

    public int CheckForStars()
    {
        if (theStackSpawner.topStack.transform.localScale.x * theStackSpawner.topStack.transform.localScale.z >= Mathf.Pow(theStackSpawner.startingStackWidth, 2) * GenerateStarMultiplier(star3Threshold))
            return 3;
        else if (theStackSpawner.topStack.transform.localScale.x * theStackSpawner.topStack.transform.localScale.z >= Mathf.Pow(theStackSpawner.startingStackWidth, 2) * GenerateStarMultiplier(star2Threshold))
            return 2;
        else if (theStackSpawner.topStack.transform.localScale.x * theStackSpawner.topStack.transform.localScale.z >= Mathf.Pow(theStackSpawner.startingStackWidth, 2) * GenerateStarMultiplier(star1Threshold))
            return 1;
        else
            return 0;
    }

    float GenerateStarMultiplier(float k)
    {
        float mult = k * Mathf.Pow(theLevelController.levelRequirements[theLevelController.level], -0.25f);
        return mult;
    }
}
