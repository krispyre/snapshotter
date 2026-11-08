using UnityEngine;

public class VentExitLower : MonoBehaviour
{
    [SerializeField] LayerSwitcher layerSwitcher;
    [SerializeField] float playerZ = 0;

    public void ExitVent(Collider player)
    {
        if (layerSwitcher != null)
            layerSwitcher.Apply();

        CharacterController cc = player.GetComponent<CharacterController>();

        Transform body = player.transform;

        cc.enabled = false;
        Vector3 newPos = body.position;
        newPos.z = playerZ;
        body.position = newPos;
        cc.enabled = true;
    }
}
