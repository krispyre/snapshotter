using UnityEngine;

[CreateAssetMenu(fileName = "ClawParams", menuName = "Player/ClawParams")]
public class ClawParams : ScriptableObject
{
    // ready? shoot => hit  => pull => cling => release
    //                 miss => return => ready
    [Header("claw debug")]
    public int arriveTime = 10;
    public int pullTime = 10;
    public float armLength = 1f;
}