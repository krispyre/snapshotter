using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
public class CamConfinerSetter : CinemachineConfiner3D
{
    public PlayerHitboxMgmt playerHb;
    [SerializeField, ReadOnlyInspector] private Stack<Collider> cbStack = new Stack<Collider>();

    private void OnEnable()
    {
        if (playerHb != null) playerHb.OnTriggerEntered += PushBound;
        if (playerHb != null) playerHb.OnTriggerExited += PopBound;
    }

    private void OnDisable()
    {
        if (playerHb != null) playerHb.OnTriggerEntered -= PushBound;
        if (playerHb != null) playerHb.OnTriggerExited -= PopBound;
    }
    void Start()
    {
        GameObject cb = GameObject.FindGameObjectsWithTag("CameraBound")[0];
        SetBounds(cb.GetComponent<Collider>());

    }
    private void PushBound(Collider trigger)
    {
        if (trigger.TryGetComponent(out CameraBoundTrigger cbt))
        {
            cbStack.Push(cbt.bound);
            SetBounds(cbStack.Peek());
        }
    }
    private void PopBound(Collider trigger)
    {
        if (trigger.TryGetComponent(out CameraBoundTrigger cbt))
        {
            Transform parent = cbStack.Peek().transform.parent;
            if (trigger.transform == cbStack.Peek().transform.parent)
            {
                cbStack.Pop();
            }
            SetBounds(cbStack.Peek());
        }
    }

    [ExecuteAlways]
    public void SetBounds(Collider bound)
    {
        BoundingVolume = bound;

    }
}
