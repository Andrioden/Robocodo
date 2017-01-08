using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using Assets.GameLogic;

public class WorldController : NetworkBehaviour
{
    public GameObject groundPrefab;
    public GameObject playerCityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;
    public GameObject foodNodePrefab;
    public GameObject harvesterRobotPrefab;
    public GameObject combatRobotPrefab;
    public GameObject transporterRobotPrefab;
    public GameObject purgeRobotPrefab;

    private GameObject groundGameObject;

    public WorldBuilder worldBuilder;

    private PlayerColorManager playerColorManager = new PlayerColorManager();

    [SyncVar]
    private int width;
    public int Width { get { return width; } }
    [SyncVar]
    private int height;
    public int Height { get { return height; } }

    private List<ResourceController> _resourceControllers = new List<ResourceController>();

    private Transform worldParent;
    private bool classIsUsedAsDemo = false;

    public static WorldController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }

        ScenarioSetup.RegisterWorldController(this);
    }

    // Use this for initialization
    private void Start()
    {
        Time.timeScale = 1;
        SpawnAndAdjustGround();
    }

    private void OnDestroy()
    {
        Destroy(groundGameObject);
    }

    public void BuildWorld(int width, int height, int matchSize)
    {
        this.width = width;
        this.height = height;

        worldBuilder = new WorldBuilder(width, height, matchSize, 10, 10, 10);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                InventoryItem itemFromTileType = InventoryItem(worldBuilder.Tiles[x, z]);
                if (itemFromTileType != null)
                    SpawnResourceNode(itemFromTileType, x, z);
            }
        }

        InfectionManager.instance.Initialize(width, height, worldBuilder.GetCityOrReservedCoordinates());
    }

    private InventoryItem InventoryItem(TileType tileType)
    {
        if (tileType == TileType.CopperNode)
            return new CopperItem();
        else if (tileType == TileType.IronNode)
            return new IronItem();
        else if (tileType == TileType.FoodNode)
            return new FoodItem();
        else
            return null;
    }

    public void BuildWorldDemoWorld(int width, int height, Transform demoWorldParent)
    {
        classIsUsedAsDemo = true;
        worldParent = demoWorldParent;
        BuildWorld(width, height, 10);
        SpawnPlayer(null);
        SpawnAndAdjustGround();
    }

    // [Server] enforced with inline code check
    public GameObject SpawnPlayer(NetworkConnection conn)
    {
        if (!IsServerOrDemo())
            return null;

        var playerPos = worldBuilder.GetNextPlayerPosition();
        GameObject playerCityGameObject = (GameObject)Instantiate(playerCityPrefab, new Vector3(playerPos.x, 0, playerPos.z), Quaternion.identity);

        var playerCityController = playerCityGameObject.GetComponent<PlayerCityController>();
        if (playerCityController != null)
        {
            playerCityController.ownerConnectionId = conn.connectionId.ToString();
            playerCityController.SetColor(playerColorManager.GetNextColor());
        }

        if (worldParent != null)
            playerCityGameObject.transform.parent = worldParent;

        /* NOTE: Always set properties before spawning object, if not there will be a delay before all clients get the values. */
        if (NetworkServer.active)
            NetworkServer.AddPlayerForConnection(conn, playerCityGameObject, 0); // playerControllerId hardcoded to 0 because we dont know what it is used for

        ScenarioSetup.Run(NetworkPanel.instance.gameModeDropdown.value, conn, playerCityGameObject);

        return playerCityGameObject;
    }

    public GameObject SpawnCombatRobotWithClientAuthority(NetworkConnection conn, int x, int z, PlayerCityController playerCity)
    {
        return SpawnObjectWithClientAuthority(conn, combatRobotPrefab, x, z, playerCity);
    }

    public GameObject SpawnHarvesterRobotWithClientAuthority(NetworkConnection conn, int x, int z, PlayerCityController playerCity)
    {
        return SpawnObjectWithClientAuthority(conn, harvesterRobotPrefab, x, z, playerCity);
    }

    public GameObject SpawnTransporterRobotWithClientAuthority(NetworkConnection conn, int x, int z, PlayerCityController playerCity)
    {
        return SpawnObjectWithClientAuthority(conn, transporterRobotPrefab, x, z, playerCity);
    }

    public GameObject SpawnPurgeRobotWithClientAuthority(NetworkConnection conn, int x, int z, PlayerCityController playerCity)
    {
        return SpawnObjectWithClientAuthority(conn, purgeRobotPrefab, x, z, playerCity);
    }

    // [Server] enforced with inline code check
    private GameObject SpawnObjectWithClientAuthority(NetworkConnection conn, GameObject prefab, int x, int z, PlayerCityController playerCity)
    {
        if (!IsServerOrDemo())
            return null;

        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        if (worldParent != null)
            newGameObject.transform.parent = worldParent;

        /* NOTE: Always set properties before spawning object, if not there will be a delay before all clients get the values. */

        if (NetworkServer.active)
            NetworkServer.SpawnWithClientAuthority(newGameObject, conn);

        var ownedObject = newGameObject.GetComponent<IOwned>();
        if (ownedObject != null)
            ownedObject.SetAndSyncOwnerCity(conn.connectionId.ToString()); // I wonder if there is some race condition where this method runs to fast, before clients have the actualy objects. Ending up with not syncing the owner.

        playerCity.AddOwnedGameObject(newGameObject);

        return newGameObject;
    }

    // [Server] enforced with inline code check
    public GameObject SpawnResourceNode(InventoryItem item, int x, int z)
    {
        if (!IsServerOrDemo())
            return null;

        GameObject resurceGameObject = SpawnObject(InventoryPrefab(item), x, z);
        ResourceController resourceController = resurceGameObject.GetComponent<ResourceController>();
        resourceController.resourceType = item.Serialize();

        _resourceControllers.Add(resourceController);

        return resurceGameObject;
    }

    private GameObject InventoryPrefab(InventoryItem item)
    {
        if (item is IronItem)
            return ironNodePrefab;
        else if (item is CopperItem)
            return copperNodePrefab;
        else if (item is FoodItem)
            return foodNodePrefab;
        else
            throw new Exception("Not prefab found for inventory item " + item);
    }

    // [Server] enforced with inline code check
    public GameObject SpawnObject(GameObject prefab, int x, int z)
    {
        if (!IsServerOrDemo())
            return null;

        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        if (worldParent != null)
            newGameObject.transform.parent = worldParent;

        /* NOTE: Always set properties before spawning object, if not there will be a delay before all clients get the values. */

        if (NetworkServer.active)
            NetworkServer.Spawn(newGameObject);

        return newGameObject;
    }

    /// <summary>
    /// Returns false if no node was found
    /// </summary>
    [Server]
    public string HarvestFromNode(float x, float z)
    {
        ResourceController resourceController = _resourceControllers.Find(r => r.transform.position.x == x && r.transform.position.z == z);
        if (resourceController != null)
        {
            resourceController.HarvestOneResourceItem();
            if (resourceController.GetRemainingResourceItems() <= 0)
            {
                _resourceControllers.Remove(resourceController);
                Destroy(resourceController.gameObject);
            }
            return resourceController.resourceType;
        }
        else
            return null;
    }

    public void SpawnAndAdjustGround()
    {
        float xPosition = (width / 2f) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles
        float zPosition = (height / 2f) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles

        groundGameObject = (GameObject)Instantiate(groundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        groundGameObject.name = "Ground_NotNetwork";

        if (worldParent != null)
            groundGameObject.transform.parent = worldParent;

        groundGameObject.transform.localScale = new Vector3(width / 10f, 1, height / 10f);
        groundGameObject.transform.position = new Vector3(xPosition, -0.001f, zPosition);

        groundGameObject.GetComponent<TextureTilingController>().RescaleTileTexture();
    }

    public PlayerCityController FindPlayerCityController(string connectionID)
    {
        // I belive finding by tag is a quick unity action and not neccesary to cache
        return GameObject.FindGameObjectsWithTag("PlayerCity")
            .Select(go => go.GetComponent<PlayerCityController>())
            .Where(p => p != null && p.ownerConnectionId == connectionID).FirstOrDefault();
    }

    private bool IsServerOrDemo()
    {
        if (classIsUsedAsDemo)
            return true;
        else if (isServer)
            return true;
        else
        {
            Debug.LogWarning("Method is called by a non-server. Stopping method excecution.");
            return false;
        }
    }
}
