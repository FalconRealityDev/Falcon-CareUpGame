using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToGroupVR : MonoBehaviour
{

    public string walkToGroupName = "";
    public string description;
    PlayerScript player;
    public Transform teleportationAnchor;
    private void Start()
    {
        player = GameObject.FindObjectOfType<PlayerScript>();
    }

    public Transform GetTeleportationAnchor()
    {
        return teleportationAnchor;
    }
    
    public void PlayerWalkedIn()
    {
        if (player != null)
            player.UpdateWalkToGroup(walkToGroupName);
            
        DebugScreenControl debugScreenControl = GameObject.FindObjectOfType<DebugScreenControl>();
        if (debugScreenControl != null)
        {
            debugScreenControl.UpdateScreenPositionToWTG(walkToGroupName);
        }
    } 
}
