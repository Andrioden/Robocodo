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
    public string ownerConnectionId = "";

    [SyncVar]
    public bool hasLost = false;

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

    private PopulationManager populationManager;
    public PopulationManager PopulationManager { get { return populationManager; } }

    [SyncVar]
    private int health;
    public int Health { get { return health; } }

    [SyncVar]
    private int energy;
    public int Energy { get { return energy; } }

    public static int Settings_StartHealth = 10;
    public static int Settings_MaxEnergyStorage = 200;


    // Use this for initialization
    private void Start()
    {
        health = Settings_StartHealth;

        if (isLocalPlayer)
        {
            ResourcePanel.instance.RegisterLocalPlayerCity(this);
            CmdRegisterPlayerNick(NetworkPanel.instance.nickInput.text);
        }

        populationManager = gameObject.GetComponent<PopulationManager>();
        if (isServer)
        {
            populationManager.Initialize(this);
            WorldTickController.instance.OnTick += Tick;
        }

        if (hasLost)
            bodyMeshRenderer.enabled = false;
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

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        AdjustCameraRelativeToPlayer();
    }

    private void Tick()
    {
        energy = Math.Min(Settings_MaxEnergyStorage, energy + Settings.City_Energy_RechargedPerTick);
    }

    public int DrainEnergy(int maxDrain)
    {
        int drained = Math.Min(energy, maxDrain);
        energy -= drained;
        return drained;
    }

    public void Click()
    {
        if (hasAuthority)
        {
            PlayerCityPanel.instance.Show(this);
        }
    }

    public ClickablePriority ClickPriority()
    {
        return ClickablePriority.High;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int GetItemCount<T>()
    {
        return inventory.Count(i => i.GetType() == typeof(T));
    }

    public void AddOwnedGameObject(GameObject go)
    {
        ownedGameObjects.Add(go);
    }

    [Command]
    public void CmdBuyHarvesterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(HarvesterRobotController.Settings_cost()))
        {
            WorldController.instance.SpawnHarvesterRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(HarvesterRobotController.Settings_cost());
            TargetIndicateSuccessfulPurchase(connectionToClient, HarvesterRobotController.Settings_name);
        }
    }

    [Command]
    public void CmdBuyCombatRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(CombatRobotController.Settings_cost()))
        {
            WorldController.instance.SpawnCombatRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(CombatRobotController.Settings_cost());
            TargetIndicateSuccessfulPurchase(connectionToClient, CombatRobotController.Settings_name);
        }
    }

    [Command]
    public void CmdBuyTransporterRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(TransporterRobotController.Settings_cost()))
        {
            WorldController.instance.SpawnTransporterRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(TransporterRobotController.Settings_cost());
            TargetIndicateSuccessfulPurchase(connectionToClient, TransporterRobotController.Settings_name);
        }
    }

    [Command]
    public void CmdBuyPurgeRobot()
    {
        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(PurgeRobotController.Settings_cost()))
        {
            WorldController.instance.SpawnPurgeRobotWithClientAuthority(connectionToClient, X, Z, this);
            RemoveResources(PurgeRobotController.Settings_cost());
            TargetIndicateSuccessfulPurchase(connectionToClient, PurgeRobotController.Settings_name);
        }
    }

    /// <summary>
    /// Returns the items not added to the inventory
    /// </summary>
    [Server]
    public List<InventoryItem> TransferToInventory(List<InventoryItem> items)
    {
        inventory.AddRange(items);
        RpcSyncInventory(InventoryItem.Serialize(inventory));
        return new List<InventoryItem>(); // No items was not added
    }

    [Server]
    public void RemoveResources(Cost cost)
    {
        for (int c = 0; c < cost.Copper; c++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(CopperItem)));
        for (int i = 0; i < cost.Iron; i++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(IronItem)));
        for (int i = 0; i < cost.Food; i++)
            inventory.RemoveAt(inventory.FindIndex(x => x.GetType() == typeof(FoodItem)));

        RpcSyncInventory(InventoryItem.Serialize(inventory));
    }

    [ClientRpc]
    private void RpcSyncInventory(string[] itemCounts)
    {
        inventory = InventoryItem.Deserialize(itemCounts);
    }

    public bool CanAfford(Cost cost)
    {
        return CanAffordReturnMissing(cost).Count == 0;
    }

    /// <summary>
    /// Returns the serialized string for resource types the player(city) is missing
    /// </summary>
    public List<string> CanAffordReturnMissing(Cost cost)
    {
        List<string> missing = new List<string>();

        if (cost.Copper > GetItemCount<CopperItem>())
            missing.Add(CopperItem.SerializedType);
        if (cost.Iron > GetItemCount<IronItem>())
            missing.Add(IronItem.SerializedType);
        if (cost.Food > GetItemCount<FoodItem>())
            missing.Add(FoodItem.SerializedType);

        return missing;
    }

    [Client]
    public bool CanAffordFlashIfNot(Cost cost)
    {
        List<string> missingResources = CanAffordReturnMissing(cost);

        foreach (string resourceString in missingResources)
            ResourcePanel.instance.FlashMissingResource(resourceString);

        return missingResources.Count == 0;
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

    private void AdjustCameraRelativeToPlayer()
    {
        RTSCamera.instance.PositionRelativeTo(transform);
    }

    public void ShowPopupForOwner(string text, Vector3 position, TextPopup.ColorType colorType)
    {
        if (isServer)
            TargetShowPopup(connectionToClient, text, position, colorType.Color());
        else
            TextPopupManager.instance.ShowPopupGeneric(text, position, colorType.Color());
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

    [TargetRpc]
    private void TargetIndicateSuccessfulPurchase(NetworkConnection target, string robotTypeName)
    {
        PlayerCityPanel.instance.buildMenu.IndicateSuccessfulPurchase(robotTypeName);
    }

    [Client]
    public double GetRelativeInfection()
    {
        double distanceAdjustedInfection = 0.0;

        foreach (TileInfection ti in InfectionManager.instance.TileInfections)
            distanceAdjustedInfection += Math.Min(Settings.World_Infection_MaxInfectionLossImpactPerTile, ti.Infection / Math.Pow(MathUtils.Distance(ti.X, ti.Z, (int)X, (int)Z), 3));

        return Math.Round(distanceAdjustedInfection, 1);
    }

    [Server]
    public void SetColor(Color32 teamColor)
    {
        hexColor = Utils.ColorToHex(teamColor);
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

    [Server]
    public void Lost()
    {
        hasLost = true;
        bodyMeshRenderer.enabled = false;
        foreach (GameObject go in ownedGameObjects)
            Destroy(go);
        RpcLost();

        WorldController.instance.SpawnObject(playerCityRubblePrefab, (int)transform.position.x, (int)transform.position.z);
    }

    [ClientRpc]
    private void RpcLost()
    {
        bodyMeshRenderer.enabled = false;

        StackingRobotsOverhangManager.instance.DestroyAll();
    }

}