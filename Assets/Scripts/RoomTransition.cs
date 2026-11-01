using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    /* this class are for objects that move player out of the room, see RoomTransitionMgr*/

    public string targetSceneName;
    public string spawnID;

// collider passed dynamically call this in oninteract
    public void TriggerTransition(Collider player){
        Debug.Log("triger rom transiton");
        RoomTransitionMgr manager = player.GetComponentInParent<RoomTransitionMgr>();
        if  (manager != null){
            manager.EnterRoom(targetSceneName, spawnID);
        }

    }
}
