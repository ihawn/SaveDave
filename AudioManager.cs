using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    LevelController theLevelController;
    StackSpawner theStackSpawner;
    UIController theUIController;

    public Rigidbody davesRB;

    public AudioSource[] perfectSound;
    public AudioSource crack, cutsceneMusic, scream, thud, levelComplete, daveWhistle, daveThud, rain, spaceMusic, 
        lightning, highScore, perfectBonusAudio, bonusMissAudio, powerupAudio, bonusFullAudio;
    public AudioSource[] generalSounds; // 0 = birds, 1 = level whoosh
    public float globalEffectVolume;
    public int perfectSoundIndex;
    public float birdMaxVolume, whooshVolMultiplier, whooshPitchMultiplier, whooshPitchVerticalOffset, whooshPitchHorOffset, mult, maxBirdTweetHeight,
        screamVolume, velocityToScream, levelCompleteVolume, startLowPassHeight, endLowPassHeight, cutoffMult, maxCutoff, minCuttof, lowPassLerpSpeed,
        lightningVolume, highScoreVolume, perfectBonusAudioVolume, perfectMissVolume, powerupCollectionVolume, bonusFullVolume, perfectSoundVolume,
        maxSpaceMusicVolume;

    bool canScream;

    AudioLowPassFilter[] lowPass;

    public float spaceMusicFadeSpeed;

    // Start is called before the first frame update
    void Start()
    {
        theUIController = FindObjectOfType<UIController>();
        globalEffectVolume = 1;
        perfectSoundIndex = 0;

        theStackSpawner = FindObjectOfType<StackSpawner>();
        theLevelController = FindObjectOfType<LevelController>();

        //initialize lowPass
        lowPass = new AudioLowPassFilter[perfectSound.Length + 6];

        lowPass[0] = crack.gameObject.GetComponent<AudioLowPassFilter>();
        lowPass[1] = levelComplete.gameObject.GetComponent<AudioLowPassFilter>();
        lowPass[2] = generalSounds[1].gameObject.GetComponent<AudioLowPassFilter>();
        lowPass[3] = daveWhistle.gameObject.GetComponent<AudioLowPassFilter>();
        lowPass[4] = daveThud.gameObject.GetComponent<AudioLowPassFilter>();
        lowPass[5] = powerupAudio.gameObject.GetComponent<AudioLowPassFilter>();

        for (int i = 6; i < lowPass.Length; i++)
        {
            lowPass[i] = perfectSound[i-6].gameObject.GetComponent<AudioLowPassFilter>();
        }

    }

    public void PlayBonusFull()
    {
        if (!bonusFullAudio.isPlaying)
        {
            bonusFullAudio.Play();
            bonusFullAudio.volume = bonusFullVolume * globalEffectVolume;
        }
    }

    public void PlayPowerupAudio()
    {
        if(!powerupAudio.isPlaying)
        {
            powerupAudio.Play();
            powerupAudio.volume = powerupCollectionVolume * globalEffectVolume;
            powerupAudio.pitch = 0.5f + theUIController.slide.mainSlider.value * 2.5f;
        }
    }



    public void PlayPerfectBonusAudio()
    {
        perfectBonusAudio.Play();
        perfectBonusAudio.volume = perfectBonusAudioVolume * globalEffectVolume;
    }

    public void PlayPerfectMissAudio()
    {
        bonusMissAudio.Play();
        bonusMissAudio.volume = perfectMissVolume * globalEffectVolume;
    }

    void UpdateLowPass()
    {
        float camPos = theLevelController.mainCamera.transform.position.y;

        if (camPos >= startLowPassHeight && camPos < endLowPassHeight)
        {
            for (int i = 0; i < lowPass.Length; i++)
            {
                lowPass[i].cutoffFrequency = Mathf.Lerp(lowPass[i].cutoffFrequency, minCuttof, lowPassLerpSpeed * Time.deltaTime);
            }
        }

        else if(camPos < startLowPassHeight)
        {
            SetLowPass(maxCutoff);
        }

        else if(camPos > endLowPassHeight)
        {
            SetLowPass(minCuttof);
        }
    }



    void SetLowPass(float val)
    {
        for(int i = 0; i < lowPass.Length; i++)
        {
            lowPass[i].cutoffFrequency = val;
        }
    }

    private void Update()
    {
        UpdateLowPass();

        //vary bird volume based on height

        if(theStackSpawner.gameOver && canScream && davesRB.velocity.y <= -velocityToScream)
        {
            //StartCoroutine(FadeInSound(scream, 2, screamVolume));
            if(!scream.isPlaying)
            {
                scream.Play();
                scream.volume = screamVolume * globalEffectVolume;

                canScream = false;
            }

        }
        else if(theStackSpawner.gameOver && davesRB.velocity.y > -velocityToScream)
        {
            scream.Stop();
        }
        else if(!theStackSpawner.gameOver)
        {
            canScream = true;
        }
       
    }

    public void PlayeLevelComplete()
    {
        levelComplete.Play();
        levelComplete.volume = levelCompleteVolume * globalEffectVolume;
    }

    public void PlayHighScore()
    {
        highScore.Play();
        highScore.volume = highScoreVolume * globalEffectVolume;
    }

    public void PlayWhipCrack()
    {
        if (crack.isPlaying)
            crack.Stop();
        crack.Play();
        crack.pitch = Random.Range(0.8f, 1.2f);
        crack.volume = globalEffectVolume;
    }

    public void PlayPerfectSound()
    {
        if (perfectSoundIndex >= perfectSound.Length)
            perfectSoundIndex = 0;

        if(perfectSound[perfectSoundIndex].isPlaying)
            perfectSound[perfectSoundIndex].Stop();

        perfectSound[perfectSoundIndex].Play();
        perfectSound[perfectSoundIndex].volume = perfectSoundVolume * globalEffectVolume;
        perfectSoundIndex++;
    }

    public IEnumerator FadeInSound(AudioSource audio, float speed, float maxVolume)
    {
        if (audio.isPlaying)
            audio.Stop();
        audio.Play();
        audio.volume = 0f;

        while(audio.volume < maxVolume*globalEffectVolume)
        {
            audio.volume += speed * Time.deltaTime;

            yield return null;
        }
    }

    public IEnumerator FadeOutSound(AudioSource audio, float speed)
    {
        if (audio.isPlaying)
        {
            while (audio.volume > 0)
            {
                audio.volume -= speed * Time.deltaTime;

                yield return null;
            }

            audio.Stop();
        }
    }

    IEnumerator LevelWhoosh()
    {
        GameObject cam = theLevelController.mainCamera;
        float camPos = cam.transform.position.y;
        float lastCamPos, speed;

        yield return null;
        lastCamPos = camPos;
        camPos = cam.transform.position.y;
        speed = Mathf.Abs(camPos - lastCamPos) / Time.deltaTime;


        if (speed > 0.001f)
        {
            float pitch = mult * (whooshPitchVerticalOffset + whooshPitchMultiplier * (float)System.Math.Tanh(speed - whooshPitchHorOffset) / (speed - whooshPitchHorOffset));

            generalSounds[1].Play();
            generalSounds[1].volume = 1.3f / pitch;
            generalSounds[1].pitch = pitch;
        }
    }

    public void PlayLevelWhoosh()
    {
        StartCoroutine(LevelWhoosh());
    }
}
