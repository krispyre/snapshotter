using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

[CreateAssetMenu(menuName = "Game/GameEvent")]
public class GameEvent : ScriptableObject
{
    public UnityEvent OnOpenCam;
    public UnityEvent OnShutCam;
    public UnityEvent SwitchToSpider;
    public UnityEvent SwitchToDroid;

    private bool openCam;
    public bool CamMode => openCam;
    private bool spiderMode;
    public bool PlayerMode => isSpider;

    public void Changemode(bool isSpider)
    {
        if(isSpider == spiderMode) return;
        spiderMode = isSpider;

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
            OnOpenCam?.Invoke;
        }
        else
        {
            OnShutCam?.Invoke;
        }
    }
    
}
