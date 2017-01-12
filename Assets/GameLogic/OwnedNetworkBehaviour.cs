using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public abstract class OwnedNetworkBehaviour : NetworkBehaviour
{

    [SyncVar]
    private string ownerConnectionID;
    private PlayerController __owner; // Dont use this variable directly if you dont know what you are doing
    public PlayerController Owner
    {
        // We reattempt to find the player controller, because we might get synced the ownerConnectionID before the Player Game Object.
        get
        {
            if (__owner == null)
                __owner = WorldController.instance.FindPlayerController(ownerConnectionID);
            return __owner;
        }
    }

    public PlayerController GetOwner() {
        return Owner;
    }

    public void SetOwner(PlayerController player)
    {
        __owner = player;
        ownerConnectionID = player.connectionId;
    }

}