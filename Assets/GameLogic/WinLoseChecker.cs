using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class WinLoseChecker : NetworkBehaviour
{

    // Use this for initialization
    private void Start()
    {
        WorldTickController.instance.OnAfterTick += WinLoseCheck;
    }

    [Server]
    public void WinLoseCheck()
    {
        foreach(PlayerCityController player in FindObjectsOfType<PlayerCityController>().Where(p => !p.hasLost))
        {
            LossCheck(player);
        }
    }

    [Server]
    private void LossCheck(PlayerCityController playerCity)
    {
        LossType lossType = LossType.None;

        if (playerCity.GetRelativeInfection() >= 22)
            lossType = LossType.Infection;
        else if (playerCity.Health <= 0)
            lossType = LossType.CityDestroyed;

        if (lossType != LossType.None)
        {
            RpcNotifyLoser(playerCity.ownerConnectionId, (int)lossType);
            playerCity.Lost();
        }
    }

    [ClientRpc]
    public void RpcNotifyLoser(string lostPlayerConnectionId, int lossTypeInt)
    {
        if (GameObjectUtils.FindClientsOwnPlayerCity().ownerConnectionId == lostPlayerConnectionId)
        {
            if (lossTypeInt == (int)LossType.Infection)
                LostPanel.instance.Show("The infection got to high near your city, your people got infected and everyone died, good job...");
            else if (lossTypeInt == (int)LossType.CityDestroyed)
                LostPanel.instance.Show("You lost! City destroyed!");
        }
    }
}

public enum LossType
{
    None = 0,
    Infection = 1,
    CityDestroyed = 2
}