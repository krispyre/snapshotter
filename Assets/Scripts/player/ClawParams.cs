using UnityEngine;

[CreateAssetMenu(fileName = "ClawParams", menuName = "Player/ClawParams")]
public class ClawParams : ScriptableObject
{
    // ready? shoot => hit  => pull => cling => release
    //                 miss => return => ready
    public int arriveTime = 10;
    public int pullTime = 10;
}