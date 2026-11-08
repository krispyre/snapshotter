using UnityEngine;

[CreateAssetMenu(fileName = "GameStates", menuName = "Scriptable Objects/GameStates")]
public class GameStates : ScriptableObject
{
    public bool openCam;
    public bool CamMode => openCam; // dupplication minor

    public bool SetCamMode(bool isOpenCam)
    {
        if (openCam == isOpenCam)
            return false;

        openCam = isOpenCam;
        return true;
    }
}
