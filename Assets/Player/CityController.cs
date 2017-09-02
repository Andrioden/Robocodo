using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class CityController : OwnedNetworkBehaviour, ISelectable, IHasInventory, IEnergySource
{
    public MeshRenderer bodyMeshRenderer;

    private bool isColorSet = false;

    private AudioSource audioSource;
    public ParticleSystem[] teleportParticleSystems;
    public AudioClip teleportationSound;
    public AudioClip resourceSound;

    public int X { get { return (int)gameObject.transform.position.x; } }
    public int Z { get { return (int)gameObject.transform.position.z; } }

    private List<InventoryItem> inventory = new List<InventoryItem>();

    private List<RobotController> garage = new List<RobotController>();
    public List<RobotController> Garage { get { return garage; } }

    public event Action<RobotController> OnRobotAddedToGarage = delegate { };
    public event Action<RobotController> OnRobotRemovedFromGarage = delegate { };

    private PopulationManager populationManager;
    public PopulationManager PopulationManager { get { return populationManager; } }

    [SyncVar]
    private int energy;
    public int Energy { get { return energy; } }

    public static int Settings_MaxEnergyStorage = 200;

    // Use this for initialization
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        populationManager = GetComponent<PopulationManager>();
        if (isServer)
        {
            populationManager.Initialize(this);
            WorldTickController.instance.OnTick += Tick;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isColorSet)
            SetMaterialColor();
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= Tick;
    }

    private void SetMaterialColor()
    {
        if (string.IsNullOrEmpty(Owner.hexColor))
            return;

        bodyMeshRenderer.material.color = Utils.HexToColor(Owner.hexColor);
        isColorSet = true;
    }

    public void PlayTeleportParticleSystem()
    {
        foreach (var particleSystem in teleportParticleSystems)
            particleSystem.Play();

        audioSource.PlayOneShot(teleportationSound);
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
            PlayerCityPanel.instance.Show(this);
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

    [Command]
    public void CmdBuyHarvesterRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(HarvesterRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(HarvesterRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.harvesterRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(HarvesterRobotController.Settings_cost());
        }
    }

    [Command]
    public void CmdBuyCombatRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(CombatRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(CombatRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.combatRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(CombatRobotController.Settings_cost());
        }
    }

    [Command]
    public void CmdBuyTransporterRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(TransporterRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(TransporterRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.transporterRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(TransporterRobotController.Settings_cost());
        }
    }

    [Command]
    public void CmdBuyStorageRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(StorageRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(StorageRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.storageRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(StorageRobotController.Settings_cost());
        }
    }

    [Command]
    public void CmdBuyPurgeRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(PurgeRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(PurgeRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.purgeRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(PurgeRobotController.Settings_cost());
        }
    }

    [Command]
    public void CmdBuyBatteryRobot()
    {
        ThrowExceptionIfDontHaveRobotTech(typeof(BatteryRobotController));

        // Is checked on the server so we are sure the player doesnt doubleclick and creates some race condition. So server always control spawning of robot and deduction of resourecs at the same time
        if (CanAfford(BatteryRobotController.Settings_cost()))
        {
            GameObject prefab = WorldController.instance.batteryRobotPrefab;
            WorldController.instance.SpawnObjectWithClientAuthority(prefab, X, Z, Owner);
            RemoveResources(BatteryRobotController.Settings_cost());
        }
    }

    private void ThrowExceptionIfDontHaveRobotTech(Type robotType)
    {
        if (!Owner.TechTree.IsRobotTechResearched(robotType))
            throw new Exception("Has not robot tech to build robot of type: " + robotType);
    }

    [Server]
    public bool HasOpenInventory()
    {
        return true;
    }

    /// <summary>
    /// Returns the items not added to the inventory
    /// </summary>
    [Server]
    public List<InventoryItem> AddToInventory(List<InventoryItem> items, bool playSoundEffect)
    {
        inventory.AddRange(items);
        RpcSyncInventory(InventoryItem.Serialize(inventory), items.Count, playSoundEffect);
        return new List<InventoryItem>(); // No items was not added since city has no max capacity
    }

    [Server]
    public List<InventoryItem> PickUp(int count)
    {
        List<InventoryItem> itemsPickedUp = inventory.PopLast(count);
        RpcSyncInventory(InventoryItem.Serialize(inventory), -itemsPickedUp.Count, false);
        return itemsPickedUp;
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

        RpcSyncInventory(InventoryItem.Serialize(inventory), -cost.TotalCost(), false);
    }

    /// <summary>
    /// Cant calculate inventoryChange locally because that wont work on the server which already has the correct inventory before method is called
    /// </summary>
    [ClientRpc]
    private void RpcSyncInventory(string[] itemCounts, int inventoryChange, bool playSoundEffect)
    {
        inventory = InventoryItem.Deserialize(itemCounts);

        if (playSoundEffect && inventoryChange > 0)
            PlayResouceGainedSoundEffect(inventoryChange);
    }

    private void PlayResouceGainedSoundEffect(int numberOfRepeats)
    {
        if (hasAuthority && numberOfRepeats > 0)
            StartCoroutine(AudioUtils.RepeatAudioClipCoroutine(audioSource, resourceSound, numberOfRepeats, 0.25f, 5f));
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

    [Client]
    public double GetInfectionImpactLossPercentage()
    {
        double distanceAdjustedInfectionImpact = 0.0;

        foreach (TileInfection ti in InfectionManager.instance.TileInfections)
        {
            double distance = MathUtils.Distance(ti.X, ti.Z, X, Z);
            distanceAdjustedInfectionImpact += Math.Min(Settings.World_Infection_MaxCityInfectionImpactPerTile, ti.Infection / Math.Pow(distance, 3));
        }

        return Math.Round(distanceAdjustedInfectionImpact * 100 / Settings.World_Infection_InfectionImpactLoss, 1);
    }

    public string GetName()
    {
        return "City";
    }

    public string GetSummary()
    {
        return "Owned by " + Owner.Nick;
    }
}