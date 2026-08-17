using UnityEngine;

[CreateAssetMenu(fileName = "ClawParams", menuName = "Player/ClawParams")]
public class ClawParams : ScriptableObject
{

    [Header("claw debug")]
    public float armLength = 1f;
    public int shootDelay = 1;
    public int flyTime = 10; //time for claw to arrive at target
    public int pullDelay = 5;
    public int pullTime = 20; //time for body to arrive target. should be > flytime
    public int returnTime = 13; //time for empty claw to return to body
    public int returnDelay = 2;
}