using UnityEngine;

[CreateAssetMenu(fileName = "DroidLocation", menuName = "Player/DroidLocation")]

//saves the droid duummy's location across scenes

//todo this is not saved in the save file yet
public class DroidLocation : ScriptableObject
{
    public string sceneName;
    public Vector3 lastPosition;
}
