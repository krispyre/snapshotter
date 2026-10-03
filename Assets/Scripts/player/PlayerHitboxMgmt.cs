using UnityEngine;
using UnityEngine.Events;

public class PlayerHitboxMgmt : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] public int iframeTimer;
    [SerializeField] private int iframeTime = 5;
    [SerializeField, ReadOnlyInspector] public bool isExposed = false;
    [SerializeField, ReadOnlyInspector] public bool wasExposed = false;

    public UnityEvent onPlayerHit;

    public void OnHit()
    {
        iframeTimer = iframeTime;
        onPlayerHit.Invoke();
    }

    void FixedUpdate()
    {
        if (iframeTimer > 0)
        {
            iframeTimer--;
        }
        else
        {
            iframeTimer = 0;
        }
    }
}
