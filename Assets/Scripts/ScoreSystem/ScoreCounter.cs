using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreCounter : MonoBehaviour
{
    public float startScore = 0f;
    public int startIdleTime = 0;
    public float timerInterval = 1f;
    public int streamViewrs = 0;
    public int buffDuration = 15;
    public float buff = 2f;
    private float currentScore;
    private int stealthScore;
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
        print("calculated:" + data.name);
        print("current score:" + currentScore);
        if (data.buffed)
        {
            currentBuffTime = buffDuration;
        }
        if(data.point == 0)
        {
            return;
        }
        else if(data.point > 0)
        {
            currentScore += stealthScore * data.point * (currentBuffTime == 0 ? 1:buff) * GetIdlePenalty();
        }
        else
        {
            currentScore -= (130 - stealthScore) * data.point;
        }
    }

    public void ClearIdleTime()
    {
        currentIdleTime = 0;
    }

    public void Reset()
    {
        currentScore = startScore;
        currentIdleTime = startIdleTime;
        stealthScore = 100;
    }

    void DoEachTime()
    {
        currentIdleTime ++;
        if (currentBuffTime > 0){currentBuffTime -= 1;}
    }

    float GetIdlePenalty()
    {
        return (128 - Mathf.Min(stealthScore*stealthScore/25,64))/64;
    }
}
