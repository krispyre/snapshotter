using UnityEngine;

public class VentEntrance : MonoBehaviour
{
    [SerializeField, ReadOnlyInspector] LayerSwitcher layerSwitcher;
    [SerializeField] float playerZ = 5f;

    void Awake()
    {
        layerSwitcher = GetComponentInParent<LayerSwitcher>();
    }

    public void EnterVent(Collider player)
    {
        if (layerSwitcher != null)
            layerSwitcher.Apply();

        if (player.TryGetComponent(out CharacterController cc))
        {
            Transform body = cc != null ? cc.transform : player.transform;
            if (cc != null)
                cc.enabled = false;
            Vector3 position = body.position;
            position.z = playerZ;
            body.position = position;
            if (cc != null)
                cc.enabled = true;
        }


    }
}
