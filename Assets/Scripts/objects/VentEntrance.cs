using UnityEngine;

public class VentEntrance : Interactable
{
    [SerializeField] GameObject ventLayer;
    [SerializeField] GameObject foregroundLayer;
    [SerializeField] float playerZ = 5f;

    protected override void OnAwake()
    {
        prompt = "Enter";
    }

    protected override void OnInteract(Collider player)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (ventLayer != null)
            ventLayer.SetActive(true);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null)
            cc = player.GetComponentInParent<CharacterController>();

        Transform body = cc != null ? cc.transform : player.transform;
        if (cc != null)
            cc.enabled = false;
        Vector3 position = body.position;
        position.z = playerZ;
        body.position = position;
        if (cc != null)
            cc.enabled = true;

        if (foregroundLayer != null)
            foregroundLayer.SetActive(false);
    }
}
