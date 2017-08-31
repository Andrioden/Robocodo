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
        WonOrLostDialog.instance.RegisterWinLoseChecker(instance);
        WorldTickController.instance.OnAfterTick += WinLoseCheck;
    }

    [Server]
    public void WinLoseCheck()
    {
        foreach(PlayerController player in FindObjectsOfType<PlayerController>().Where(p => !p.hasLost && p.City != null))
        {
            AllInfectionClearedCheck();
            WinCheck(player);
            LossCheck(player);
        }
    }

    [Server]
    private void LossCheck(PlayerController player)
    {
        LossType lossType = LossType.None;

        if (player.City.GetInfectionImpactLossPercentage() >= 100)
            lossType = LossType.Infection;
        else if (player.City.Health <= 0)
            lossType = LossType.CityDestroyed;
        else if (player.City.PopulationManager.Population <= 0)
            lossType = LossType.StarvedToDeath;

        if (lossType != LossType.None)
        {
            RpcNotifyLoser(player.ConnectionID, (int)lossType);
            player.LostAndDestroy();
        }
    }

    [Server]
    private void WinCheck(PlayerController player)
    {
        WinType winType = WinType.None;

        if (player.TechTree.GetFinishedVictoryTech() != null)
            winType = WinType.Technology;

        if (winType != WinType.None)
        {
            RpcNotifyWinner(player.ConnectionID, (int)winType);
            Time.timeScale = 0;
        }
    }

    [Server]
    private void AllInfectionClearedCheck()
    {
        if (InfectionManager.instance.TileInfections.Sum(ti => ti.Infection) == 0)
            RpcNotifyAllInfectionCleared();
    }

    [ClientRpc]
    public void RpcNotifyAllInfectionCleared()
    {
        if (WorldController.instance.ClientsOwnPlayer().infectionContribution == GetMaxInfectionContribution())
            OnWon(WinType.Infection_TopContributor);
        else
            OnLost(LossType.Infection_NotTopContributor);
    }

    [ClientRpc]
    public void RpcNotifyWinner(string wonPlayerConnectionId, int wonTypeInt)
    {
        if (WorldController.instance.ClientsOwnPlayer().ConnectionID == wonPlayerConnectionId)
            OnWon((WinType)wonTypeInt);
        else
            OnLost((LossType)wonTypeInt);
    }

    [ClientRpc]
    public void RpcNotifyLoser(string lostPlayerConnectionId, int lossTypeInt)
    {
        if (WorldController.instance.ClientsOwnPlayer().ConnectionID == lostPlayerConnectionId)
            OnLost((LossType)lossTypeInt);
        //TODO: On non-losing clients, maybe notify a player lost?
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
    Technology = 40,
}

public enum WinType
{
    None = 0,
    Infection_TopContributor = 11,
    Technology = 40,
}