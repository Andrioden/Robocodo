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
        foreach(PlayerController player in FindObjectsOfType<PlayerController>().Where(p => !p.hasLost && p.City != null))
        {
            LossCheck(player);
        }
    }

    [Server]
    private void LossCheck(PlayerController player)
    {
        LossType lossType = LossType.None;

        if (player.City.GetRelativeInfection() >= 22)
            lossType = LossType.Infection;
        else if (player.City.Health <= 0)
            lossType = LossType.CityDestroyed;
        else if (player.City.PopulationManager.Population <= 0)
            lossType = LossType.StarvedToDeath;

        if (lossType != LossType.None)
        {
            RpcNotifyLoser(player.connectionId, (int)lossType);
            player.Lost();
        }
    }

    [ClientRpc]
    public void RpcNotifyLoser(string lostPlayerConnectionId, int lossTypeInt)
    {
        if (GameObjectUtils.FindClientsOwnPlayer().connectionId == lostPlayerConnectionId)
        {
            if (lossTypeInt == (int)LossType.Infection)
                LostPanel.instance.Show("The infection got to high near your city, your people got infected and everyone died, good job...");
            else if (lossTypeInt == (int)LossType.CityDestroyed)
                LostPanel.instance.Show("You lost! City destroyed!");
            else if (lossTypeInt == (int)LossType.StarvedToDeath)
                LostPanel.instance.Show("You lost! Everyone died of starvation.");
        }
    }
}

public enum LossType
{
    None = 0,
    Infection = 1,
    CityDestroyed = 2,
    StarvedToDeath
}