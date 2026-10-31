using UnityEngine;

public class VentExitLower : Interactable
{
    [SerializeField] GameObject ventLayer;
    [SerializeField] GameObject foregroundLayer;
    [SerializeField] float playerZ = 0;

    protected override void OnAwake()
    {
        prompt = "Exit";
    }

    protected override void OnInteract(Collider player)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (foregroundLayer != null)
            foregroundLayer.SetActive(true);

        CharacterController cc = player.GetComponent<CharacterController>();

        Transform body = player.transform;

        cc.enabled = false;
        Vector3 newPos = body.position;
        newPos.z = playerZ;
        body.position = newPos;
        cc.enabled = true;

        if (ventLayer != null)
            ventLayer.SetActive(false);
    }
}
