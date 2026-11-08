using UnityEngine;
using Unity.Cinemachine;

public class CamConfinerSetter : CinemachineConfiner3D
{
    /** custom cinemachine confiner set for each level. */
    void Start()
    {
        GameObject cb = GameObject.FindGameObjectsWithTag("CameraBound")[0];
        BoundingVolume = cb.GetComponent<Collider>();

    }
}
