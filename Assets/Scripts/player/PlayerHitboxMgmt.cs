using UnityEngine;

public class PlayerHitboxMgmt : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] private int iframeTimer;
    [SerializeField] private int iframeTime = 5;
    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnHit()
    {
        //set for invincible frames
        iframeTimer = iframeTime;
    }

    void FixedUpdate()
    {
        ;
    }
}
