using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerCityController : NetworkBehaviour, ISelectable, IAttackable
{
    public MeshRenderer bodyMeshRenderer;

    private List<InventoryItem> inventory = new List<InventoryItem>();

    [SyncVar]
    private int health;
    public int Health { get { return health; } }

    public static int Settings_StartHealth = 10;

    // Use this for initialization
    void Start()
    {
        health = Settings_StartHealth;

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
        {
            PlayerCityPanel.instance.ShowPanel(this);
        }
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
    public void CmdBuyHarvesterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= HarvesterRobotController.Settings_CopperCost && GetIronCount() >= HarvesterRobotController.Settings_IronCost)
        {
            CmdSpawnHarvesterRobot((int)transform.position.x, (int)transform.position.z);
            RemoveResources(HarvesterRobotController.Settings_CopperCost, HarvesterRobotController.Settings_IronCost);
        }
    }

    [Command]
    public void CmdBuyCombatRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= CombatRobotController.Settings_CopperCost && GetIronCount() >= CombatRobotController.Settings_IronCost)
        {
            CmdSpawnCombatRobot((int)transform.position.x, (int)transform.position.z);
            RemoveResources(CombatRobotController.Settings_CopperCost, CombatRobotController.Settings_IronCost);
        }
    }

    [Server]
    private void CmdSpawnHarvesterRobot(int x, int z)
    {
        WorldController.instance.SpawnHarvesterRobotWithClientAuthority(connectionToClient, x, z);
    }

    [Server]
    private void CmdSpawnCombatRobot(int x, int z)
    {
        WorldController.instance.SpawnCombatRobotWithClientAuthority(connectionToClient, x, z);
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

    [Server]
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.LogFormat("Robot {0} took {1} damage and now has {2} health", name, damage, health);

        if (health <= 0)
            Destroy(gameObject);
    }

    public string GetOwner()
    {
        return connectionToClient.connectionId.ToString();
    }
}