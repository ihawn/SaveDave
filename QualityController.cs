using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QualityController : MonoBehaviour
{
    public Vector2 startingRenderRes;
    public float aveFramerate, playableFramerate;
    public int framesPerBench, qualityLevel, resDivisor;

    private UIController theUIController;

    public Vector2[] resolutions;


    public ParticleSystem[] waterfallSprayParticles, waterfallParticles;

    // Start is called before the first frame update
    void Start()
    {
        theUIController = FindObjectOfType<UIController>();

        qualityLevel = 5;
      //  QualitySettings.SetQualityLevel(qualityLevel - 1, true); // best quality - 1
        resDivisor = 1;


        if (!PlayerPrefs.HasKey("res") || !PlayerPrefs.HasKey("qual"))
            StartCoroutine(AutoSetQuality());

    }



    public void SetResolution(int selectorIndex)
    {
        PlayerPrefs.SetInt("res", selectorIndex);
        Screen.SetResolution((int) resolutions[selectorIndex].x, (int)resolutions[selectorIndex].y, true, 60);
    }

    public void SetQuality(int selectorIndex)
    {
        PlayerPrefs.SetInt("qual", selectorIndex);
        QualitySettings.SetQualityLevel(selectorIndex, true);

    }

    public void SetWaterfallQuality(int selectorIndex)
    {
        PlayerPrefs.SetInt("waterfallQual", selectorIndex);

        switch(selectorIndex)
        {
            case 0:
                ActivateWaterfallParticles(false);
                ActivateSpray(false);
                break;

            case 1:
                ActivateWaterfallParticles(false);
                ActivateSpray(true);
                break;

            case 2:
                ActivateWaterfallParticles(true);
                ActivateSpray(true);
                break;
        }
    }

    void ActivateSpray(bool val)
    {
        for (int i = 0; i < waterfallSprayParticles.Length; i++)
            waterfallSprayParticles[i].gameObject.SetActive(val);
    }

    void ActivateWaterfallParticles(bool val)
    {
        for (int i = 0; i < waterfallParticles.Length; i++)
            waterfallParticles[i].gameObject.SetActive(val);
    }

    IEnumerator AutoSetQuality()
    {
        float frameSum = 0;
        yield return new WaitForSeconds(0.2f);

        for(int i = 0; i < framesPerBench; i++)
        {
            float framerate = 1f / Time.deltaTime;
            frameSum += framerate;


            yield return null;
        }


        aveFramerate = frameSum / framesPerBench;

        if (aveFramerate < playableFramerate && qualityLevel > 0)
        {
            
            qualityLevel--;
            theUIController.resSelector.defaultIndex =  5 - qualityLevel;
            theUIController.qualSelector.defaultIndex =  qualityLevel;

            SetQuality(qualityLevel);
            SetResolution(5 - qualityLevel);

            resDivisor++;
            

            StartCoroutine(AutoSetQuality());
        }
    }
}
