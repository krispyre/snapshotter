using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    public int startIdleTime = 0;
    public float timerInterval = 1f;
    public int streamViewrs = 0;
    public int buffDuration = 15;
    public float buff = 2f;
    [SerializeField] private GameStates gameStates;
    [SerializeField] private GameObject streamPanel;
    private int currentBuffTime;
    private int currentIdleTime;
    private float timerAccumulator = 0;
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        timerAccumulator += Time.deltaTime;
        if (timerAccumulator >= timerInterval)
        {
            timerAccumulator -= timerAccumulator;
            DoEachTime();
        }
    }

    public void CalculateScore(GameObject evidenceObj)
    {
        EvidenceData data = evidenceObj.GetComponent<Evidence>().evidenceData;
        ClearIdleTime();
        if (data.buffed)
        {
            currentBuffTime = buffDuration;
        }
        if (data.point > 0)
        {
            gameStates.AddScore(gameStates.Stealth * data.point * (currentBuffTime == 0 ? 1:buff) * GetIdlePenalty());
        }
        else if (data.point < 0)
        {
            gameStates.AddScore((130 - gameStates.Stealth) * data.point);
        }

        streamPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Score: {gameStates.Score}";
    }

    public void ClearIdleTime()
    {
        currentIdleTime = 0;
    }

    public void Reset()
    {
        gameStates.AddScore(-gameStates.Score);
        gameStates.AddViewers(-gameStates.Viewers);
        gameStates.SetStealth(100f);
        currentIdleTime = startIdleTime;
    }

    void DoEachTime()
    {
        gameStates.AddViewers(-(int)(gameStates.Viewers*0.05f) + UnityEngine.Random.Range(0, 2));
        currentIdleTime ++;
        if (currentBuffTime > 0){currentBuffTime -= 1;}
        int viewerGain = (int)(gameStates.Score*GetViewerMultiplier()*0.1f);
        gameStates.AddViewers(UnityEngine.Random.Range(viewerGain/3, viewerGain));
        streamPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"Viewers: {gameStates.Viewers}";
    }

    float GetIdlePenalty()
    {
        return (128 - Mathf.Min(currentIdleTime*currentIdleTime/25,64))/64;
    }

    float GetViewerMultiplier()
    {
        return (64 - Mathf.Min(currentIdleTime*currentIdleTime/25,64))/64;
    }
}
