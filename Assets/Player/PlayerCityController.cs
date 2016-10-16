using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerCityController : NetworkBehaviour, ISelectable, IHasInventory, IEnergySource
{
    public MeshRenderer bodyMeshRenderer;
    public GameObject playerCityRubblePrefab;

    [SyncVar]
    public string owner = "";

    [SyncVar]
    private string nick;
    public string Nick { get { return nick; } }

    [SyncVar]
    public string hexColor;

    private bool isColorSet = false;

    public int X { get { return (int)gameObject.transform.position.x; } }
    public int Z { get { return (int)gameObject.transform.position.z; } }

    private List<GameObject> ownedGameObjects = new List<GameObject>();
    private List<InventoryItem> inventory = new List<InventoryItem>();
    private List<RobotController> garage = new List<RobotController>();
    public List<RobotController> Garage { get { return garage; } }

    public delegate void GarageEventHandler(RobotController robot);
    public event GarageEventHandler OnRobotAddedToGarage;
    public event GarageEventHandler OnRobotRemovedFromGarage;

    [SyncVar]
    private int health;
    public int Health { get { return health; } }

    public static int Settings_StartHealth = 10;

    // Use this for initialization
    private void Start()
    {
        health = Settings_StartHealth;

        if (isLocalPlayer)
        {
            ResourcePanel.instance.RegisterLocalPlayerCity(this);
            CmdRegisterPlayerNick(NetworkPanel.instance.nickInput.text);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isColorSet)
            SetColor();
    }

    private void SetColor()
    {
        if (string.IsNullOrEmpty(hexColor))
            return;

        bodyMeshRenderer.material.color = Utils.HexToColor(hexColor);
        isColorSet = true;
    }

    private void OnDestroy()
    {
        if (hasAuthority && health == 0) // Health check to avoid that you get an error in the editor
            LostPanel.instance.Show();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        AdjustCameraRelativeToPlayer();
    }

    public void Click()
    {
        if (hasAuthority)
        {
            PlayerCityPanel.instance.Show(this);
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

    public string GetOwner()
    {
        return owner;
    }

    public void AddOwnedGameObject(GameObject go)
    {
        ownedGameObjects.Add(go);
    }

    [Command]
    public void CmdBuyHarvesterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= HarvesterRobotController.Settings_copperCost && GetIronCount() >= HarvesterRobotController.Settings_ironCost)
        {
            WorldController.instance.SpawnHarvesterRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(HarvesterRobotController.Settings_copperCost, HarvesterRobotController.Settings_ironCost);
        }
    }

    [Command]
    public void CmdBuyCombatRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= CombatRobotController.Settings_copperCost && GetIronCount() >= CombatRobotController.Settings_ironCost)
        {
            WorldController.instance.SpawnCombatRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(CombatRobotController.Settings_copperCost, CombatRobotController.Settings_ironCost);
        }
    }

    [Command]
    public void CmdBuyTransporterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (GetCopperCount() >= TransporterRobotController.Settings_copperCost && GetIronCount() >= TransporterRobotController.Settings_ironCost)
        {
            WorldController.instance.SpawnTransporterRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(TransporterRobotController.Settings_copperCost, TransporterRobotController.Settings_ironCost);
        }
    }

    [Server]
    public void SetColor(Color32 teamColor)
    {
        hexColor = Utils.ColorToHex(teamColor);
    }

    /// <summary>
    /// Returns the items not added to the inventory
    /// </summary>
    [Server]
    public List<InventoryItem> TransferToInventory(List<InventoryItem> items)
    {
        inventory.AddRange(items);
        RpcSyncInventory(InventoryItem.SerializeList(inventory));
        return new List<InventoryItem>(); // No items was not added
    }

    public void EnterGarage(RobotController robot)
    {
        garage.Add(robot);
        if (OnRobotAddedToGarage != null)
            OnRobotAddedToGarage(robot);
    }

    public void ExitGarage(RobotController robot)
    {
        garage.Remove(robot);
        if (OnRobotAddedToGarage != null)
            OnRobotRemovedFromGarage(robot);
    }

    [Server]
    public void RemoveResources(int copper, int iron)
    {
        for (int c = 0; c < copper; c++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(CopperItem)));

        for (int i = 0; i < iron; i++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(IronItem)));

        RpcSyncInventory(InventoryItem.SerializeList(inventory));
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] itemCounts)
    {
        inventory = InventoryItem.DeserializeList(itemCounts);
    }

    private void AdjustCameraRelativeToPlayer()
    {
        RTSCamera.instance.PositionRelativeTo(transform);
    }

    [Server]
    public void ShowPopupForOwner(string text, Vector3 position, Color color)
    {
        TargetShowPopup(connectionToClient, text, position, color);
    }

    [Server]
    public void ShowPopupForAll(string text, Vector3 position, Color color)
    {
        TextPopupManager.instance.ShowPopupGeneric(text, position, color);
    }

    [TargetRpc]
    private void TargetShowPopup(NetworkConnection target, string text, Vector3 position, Color color)
    {
        TextPopupManager.instance.ShowPopupGeneric(text, position, color);
    }

    [Command]
    private void CmdRegisterPlayerNick(string nick)
    {
        List<string> currentNicks = new List<string>();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("PlayerCity"))
        {
            PlayerCityController playerCity = go.GetComponent<PlayerCityController>();
            if (playerCity != null && !string.IsNullOrEmpty(playerCity.Nick))
                currentNicks.Add(playerCity.Nick);
        }

        if (!currentNicks.Contains(nick))
            this.nick = nick;
        else
            this.nick = nick + (currentNicks.Count(n => n.Contains(nick)) + 1);
    }

}

//[Server]
//public void TakeDamage(int damage)
//{
//    health -= damage;
//    Debug.LogFormat("Robot {0} took {1} damage and now has {2} health", name, damage, health);

//    if (health <= 0)
//    {
//        foreach (GameObject go in ownedGameObjects)
//            Destroy(go);
//        Destroy(gameObject);
//        WorldController.instance.SpawnObject(playerCityRubblePrefab, (int)transform.position.x, (int)transform.position.z);
//    }
//}