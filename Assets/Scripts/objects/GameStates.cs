using UnityEngine;

[CreateAssetMenu(fileName = "GameStates", menuName = "Scriptable Objects/GameStates")]
public class GameStates : ScriptableObject
{
    [SerializeField, ReadOnlyInspector] private bool openCam;
    public bool CamMode => openCam;
    [SerializeField, ReadOnlyInspector] private bool isDroid = true;
    public bool IsDroid => isDroid;

    [Header("DroidLocation")]
    [SerializeField, ReadOnlyInspector] private string droidSceneName = "";
    public string DroidSceneName => droidSceneName;
    [SerializeField, ReadOnlyInspector] private Vector3 droidLastPosition;
    public Vector3 DroidLastPosition => droidLastPosition;

    public bool SetCamMode(bool isOpenCam)
    {
        if (openCam == isOpenCam)
            return false;

        openCam = isOpenCam;
        return true;
    }

    public bool SetIsDroid(bool isDroid)
    {
        if (this.isDroid == isDroid)
            return false;

        this.isDroid = isDroid;
        return true;
    }

    public void SetDroidLocation(string sceneName, Vector3 position)
    {
        droidSceneName = sceneName ?? "";
        droidLastPosition = position;
    }
}
