using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using System;

public class WinLoseChecker : NetworkBehaviour
{

    public event Action<LossType> OnLost = delegate { };

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
            OnLost((LossType)lossTypeInt);
    }
}

public enum LossType
{
    None = 0,
    Infection = 1,
    CityDestroyed = 2,
    StarvedToDeath
}