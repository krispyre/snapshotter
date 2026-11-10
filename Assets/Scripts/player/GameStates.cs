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
}
