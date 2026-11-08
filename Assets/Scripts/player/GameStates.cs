using UnityEngine;

[CreateAssetMenu(fileName = "GameStates", menuName = "Scriptable Objects/GameStates")]
public class GameStates : ScriptableObject
{
    public bool openCam;
    public bool CamMode => openCam; // dupplication minor

    [Header("DroidLocation")]
    public string droidSceneName = "";
    public string DroidSceneName => droidSceneName;
    public Vector3 droidLastPosition;
    public Vector3 DroidLastPosition => droidLastPosition;

    public bool SetCamMode(bool isOpenCam)
    {
        if (openCam == isOpenCam)
            return false;

        openCam = isOpenCam;
        return true;
    }

    public void SetDroidLocation(string sceneName, Vector3 position)
    {
        droidSceneName = sceneName ?? "";
        droidLastPosition = position;
    }
}
