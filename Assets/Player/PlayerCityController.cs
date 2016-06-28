﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerCityController : NetworkBehaviour, IClickable
{
    public MeshRenderer bodyMeshRenderer;

    private List<InventoryItem> inventory = new List<InventoryItem>();

    // Use this for initialization
    void Start()
    {
        if (isLocalPlayer)
            ResourcePanel.instance.RegisterLocalPlayerCity(this);

        if (isServer)
            AddToInventory(new List<InventoryItem>()
            {
                new CopperItem(),
                new IronItem(),
                new IronItem(),
                new IronItem(),
            });
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        bodyMeshRenderer.material.color = Color.blue;

        AdjustCameraRelativeToPlayer();
    }

    public void Click()
    {
        if (hasAuthority)
            CmdBuyHarvesterRobot();
    }

    public int GetCopperCount()
    {
        return inventory.Count(i => i.GetType() == typeof(CopperItem));
    }

    public int GetIronCount()
    {
        return inventory.Count(i => i.GetType() == typeof(IronItem));
    }

    [Command]
    private void CmdBuyHarvesterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= HarvesterRobotController.CopperCost && GetIronCount() >= HarvesterRobotController.IronCost)
        {
            CmdSpawnHarvester((int)transform.position.x, (int)transform.position.z);
            RemoveResources(HarvesterRobotController.CopperCost, HarvesterRobotController.IronCost);
        }
    }

    [Server]
    public void CmdSpawnHarvester(int x, int z)
    {
        WorldController.instance.SpawnHarvesterWithClientAuthority(connectionToClient, x, z);
    }

    [Server]
    public void AddToInventory(List<InventoryItem> items)
    {
        inventory.AddRange(items);
        RpcSyncInventory(inventory.Select(i => i.Serialize()).ToArray());
    }

    [Server]
    public void RemoveResources(int copper, int iron)
    {
        for (int c = 0; c < copper; c++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(CopperItem)));

        for (int i = 0; i < iron; i++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(IronItem)));

        RpcSyncInventory(inventory.Select(i => i.Serialize()).ToArray());
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] itemStrings)
    {
        inventory = new List<InventoryItem>();

        foreach (string itemString in itemStrings)
        {
            if (itemString == CopperItem.SerializedType)
                inventory.Add(new CopperItem());
            else if (itemString == IronItem.SerializedType)
                inventory.Add(new IronItem());
            else
                throw new Exception("Forgot to add deserialization support for InventoryType: " + itemString);
        }
    }

    private void AdjustCameraRelativeToPlayer()
    {
        RTSCamera camera = Camera.main.transform.parent.GetComponent<RTSCamera>();
        camera.PositionRelativeToPlayer(transform);
        camera.transform.localPosition += new Vector3(0, -15, 0);
    }

}