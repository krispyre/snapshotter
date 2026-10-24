using UnityEngine;
using System;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/GameEvent")]
public class GameEvent : ScriptableObject
{
    public UnityEvent OnOpenCam;
    public UnityEvent OnShutCam;
    public UnityEvent SwitchToSpider;
    public UnityEvent SwitchToDroid;

    private bool openCam;
    public bool CamMode => openCam;
    private bool spiderMode;
    public bool PlayerModeIsSpider => spiderMode;

    public void Changemode(bool isSpider)
    {
        if(isSpider == spiderMode) return;
        spiderMode = isSpider;
        ToggleCamera(false); // Close the camera when switching modes

        if (isSpider)
        {
            SwitchToSpider?.Invoke();
        }
        else
        {
            SwitchToDroid?.Invoke();
        }
    }

    public void ToggleCamera(bool isOpenCam)
    {
        if (openCam == isOpenCam) return;
        openCam = isOpenCam;

        if (isOpenCam)
        {
            OnOpenCam?.Invoke();
        }
        else
        {
            OnShutCam?.Invoke();
        }
    }
    
}
