using UnityEngine;

[CreateAssetMenu(fileName = "GameStates", menuName = "Scriptable Objects/GameStates")]
public class GameStates : ScriptableObject
{
    // When main is merged, move the droid-dummy location asset into here.
    // Keep values only, such as a position. Do not store scene object references.

    [SerializeField, ReadOnlyInspector] private bool openCam;
    public bool CamMode => openCam;
    [SerializeField, ReadOnlyInspector] private bool isDroid = true;
    public bool IsDroid => isDroid;

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
}
