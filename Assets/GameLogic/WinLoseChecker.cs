using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;

public class WinLoseChecker : NetworkBehaviour
{

    public event Action<LossType> OnLost = delegate { };
    public event Action<WinType> OnWon = delegate { };

    public static WinLoseChecker instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

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
            AllInfectionClearedCheck();
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
            player.LostAndDestroy();
        }
    }

    [Server]
    private void AllInfectionClearedCheck()
    {
        if (InfectionManager.instance.TileInfections.Sum(ti => ti.Infection) == 0)
            RpcNotifyAllInfectionCleared();
    }

    [ClientRpc]
    public void RpcNotifyLoser(string lostPlayerConnectionId, int lossTypeInt)
    {
        if (WorldController.instance.FindClientsOwnPlayer().connectionId == lostPlayerConnectionId)
            OnLost((LossType)lossTypeInt);
        //TODO: On non-losing clients, maybe notify a player lost?
    }

    [ClientRpc]
    public void RpcNotifyAllInfectionCleared()
    {
        if (WorldController.instance.FindClientsOwnPlayer().infectionContribution == GetMaxInfectionContribution())
            OnWon(WinType.Infection_TopContributor);
        else
            OnLost(LossType.Infection_NotTopContributor);
    }

    private double GetMaxInfectionContribution()
    {
        return WorldController.instance.FindPlayerControllers().Max(p => p.infectionContribution);
    }

}

public enum LossType
{
    None = 0,
    Infection = 10,
    Infection_NotTopContributor = 11,
    CityDestroyed = 20,
    StarvedToDeath = 30,
}

public enum WinType
{
    None = 0,
    Infection_TopContributor = 11
}