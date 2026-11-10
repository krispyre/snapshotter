using UnityEngine;

[CreateAssetMenu(fileName = "GameStates", menuName = "Scriptable Objects/GameStates")]
public class GameStates : ScriptableObject
{
    public bool openCam;
    public bool CamMode => openCam; // dupplication minor
    private float currentScore = 0f;
    public float Score => currentScore;
    private int currentViewers = 0;
    public int Viewers => currentViewers;
    private float currentStealth = 100;
    public float Stealth => currentStealth;

    public bool SetCamMode(bool isOpenCam)
    {
        if (openCam == isOpenCam)
            return false;

        openCam = isOpenCam;
        return true;
    }

    public void AddScore(float score)
    {
        currentScore += score;
        if (currentScore < 0f)
            currentScore = 0f;
    }

    public void AddViewers(int viewers)
    {
        currentViewers += viewers;
        if (currentViewers < 0)
            currentViewers = 0;
    }

    public void SetStealth(float stealth)
    {
        currentStealth = stealth;
        if (currentStealth < 0)
            currentStealth = 0;
        else if (currentStealth > 100)
            currentStealth = 100;
    }
}
