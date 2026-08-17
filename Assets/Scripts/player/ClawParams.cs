using UnityEngine;

[CreateAssetMenu(fileName = "ClawParams", menuName = "Player/ClawParams")]
public class ClawParams : ScriptableObject
{

    [Header("claw debug")]
    public int arriveTime = 10;//frame count
    public int pullTime = 10;//frame count
    public float armLength = 1f;
}