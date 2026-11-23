using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;
public class CamConfinerSetter : MonoBehaviour
{
    public PlayerHitboxMgmt playerHb;
    private Stack<CameraBoundTrigger> triggerStack = new();
    [SerializeField, ReadOnlyInspector] private List<CameraBoundTrigger> triggerStackView = new();
    private void OnEnable()
    {
        if (playerHb != null) playerHb.OnTriggerEntered += PushBoundedVcam;
        if (playerHb != null) playerHb.OnTriggerExited += PopBoundedVcam;
    }

    private void OnDisable()
    {
        if (playerHb != null) playerHb.OnTriggerEntered -= PushBoundedVcam;
        if (playerHb != null) playerHb.OnTriggerExited -= PopBoundedVcam;
    }
    void Start()
    {
        CameraBoundTrigger cbt = FindAnyObjectByType<CameraBoundTrigger>();
        Debug.Log(cbt.name);
        SetBoundedVcam(cbt.vcam);

    }
    private void PushBoundedVcam(Collider trigger)
    {
        if (!trigger.TryGetComponent(out CameraBoundTrigger cbt)) return;

        Debug.Log("push " + cbt.name);
        triggerStack.Push(cbt);
        triggerStackView.Insert(0, cbt);
        SetBoundedVcam(triggerStack.Peek().vcam);
    }
    private void PopBoundedVcam(Collider trigger)
    {
        if (!trigger.TryGetComponent(out CameraBoundTrigger cbt)) return;
        if (triggerStack.Count == 0) return;
        if (cbt.name != triggerStack.Peek().name) return;

        triggerStack.Peek().vcam.Priority = 0;
        triggerStack.Pop();
        triggerStackView.RemoveAt(0);
        if (triggerStack.Count > 0)
            SetBoundedVcam(triggerStack.Peek().vcam);
    }

    [ExecuteAlways]
    public void SetBoundedVcam(CinemachineCamera boundedVcam)
    {
        boundedVcam.Priority = 10;
        boundedVcam.Target.TrackingTarget = transform;

    }
}
