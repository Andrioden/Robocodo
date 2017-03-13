using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using Assets.GameLogic;
using Robocodo.AndreAI;

public class WorldController : NetworkBehaviour
{
    public GameObject groundPrefab;
    public GameObject playerPrefab;
    public GameObject cityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;
    public GameObject foodNodePrefab;
    public GameObject harvesterRobotPrefab;
    public GameObject combatRobotPrefab;
    public GameObject transporterRobotPrefab;
    public GameObject purgeRobotPrefab;
    public GameObject storageRobotPrefab;

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

        NetworkPanel.instance.SetIngameUIActive(true);
    }

    private void OnDestroy()
    {
        Destroy(groundGameObject);
    }

    public void BuildWorld(int width, int height, int matchSize, NoiseConfig noiseConfig)
    {
        this.width = width;
        this.height = height;

        worldBuilder = new WorldBuilder(width, height, matchSize, noiseConfig);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                InventoryItem itemFromTileType = InventoryItem(worldBuilder.Tiles[x, z]);
                if (itemFromTileType != null)
                    SpawnResourceNode(itemFromTileType, x, z);
            }
        }
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

    public void BuildWorldDemoWorld(int width, int height, Transform demoWorldParent, NoiseConfig noiseConfig)
    {
        classIsUsedAsDemo = true;
        worldParent = demoWorldParent;
        BuildWorld(width, height, 10, noiseConfig);
        SpawnAndAdjustGround();
    }

    // [Server] enforced with inline code check
    public GameObject SpawnPlayer(NetworkConnection conn)
    {
        if (!IsServerOrDemo())
            return null;

        var playerPos = worldBuilder.GetNextPlayerPosition();

        GameObject playerGO = Instantiate(playerPrefab, new Vector3(playerPos.x, 0, playerPos.z), Quaternion.identity);

        if (worldParent != null)
            playerGO.transform.parent = worldParent;

        PlayerController player = playerGO.GetComponent<PlayerController>();
        player.connectionId = conn.connectionId.ToString();
        player.SetColor(playerColorManager.GetNextColor());

        playerGO.AddComponent<AndreAI>();

        /* NOTE: Always set properties before spawning object, if not there will be a delay before all clients get the values. */
        if (NetworkServer.active)
            NetworkServer.AddPlayerForConnection(conn, playerGO, 0); // playerControllerId hardcoded to 0 because we dont know what it is used for
        else
            Debug.LogWarning("NetworkServer not active");

        SpawnObject(cityPrefab, playerPos.x, playerPos.z, player, conn);

        ScenarioSetup.Run(NetworkPanel.instance.GetSelectedScenarioChoice(), conn, player);

        return playerGO;
    }

    [Server]
    public GameObject SpawnAI(string name)
    {
        Debug.Log("Spawning AI: " + name);

        var aiPos = worldBuilder.GetNextPlayerPosition();

        GameObject aiPlayerGO = SpawnObject(playerPrefab, aiPos.x, aiPos.z);

        PlayerController player = aiPlayerGO.GetComponent<PlayerController>();
        player.connectionId = name.Replace(" ", "_");
        player.SetColor(playerColorManager.GetNextColor());

        SpawnObject(cityPrefab, aiPos.x, aiPos.z, player);

        ScenarioSetup.Run(NetworkPanel.instance.GetSelectedScenarioChoice(), null, player); // A bit dirty atm, sending in the hosting players connection for AIs for spawning.

        aiPlayerGO.AddComponent<AndreAI>();

        return aiPlayerGO;
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

        // Noise Visualisaztion helper code
        //resurceGameObject.transform.localScale = new Vector3(resurceGameObject.transform.localScale.x * 2, 1, resurceGameObject.transform.localScale.z * 2);
        //resurceGameObject.transform.localScale = new Vector3(resurceGameObject.transform.localScale.x * 2, worldBuilder.noiseMap[x, z] * 30, resurceGameObject.transform.localScale.z * 2);

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

    public GameObject SpawnObjectWithClientAuthority(GameObject prefab, int x, int z, PlayerController owner)
    {
        return SpawnObject(prefab, x, z, owner, owner.connectionToClient);
    }

    /// <summary>
    /// All-doing method to spawn objects
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="owner">If given the object will be owned by an Player or AI</param>
    /// <param name="conn">If given this is a human controlled player</param>
    /// <returns></returns>
    // [Server] enforced with inline code check
    public GameObject SpawnObject(GameObject prefab, int x, int z, PlayerController owner = null, NetworkConnection conn = null)
    {
        if (!IsServerOrDemo())
            return null;

        GameObject newGO = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        if (worldParent != null)
            newGO.transform.parent = worldParent;

        if (owner != null)
        {
            var ownedObject = newGO.GetComponent<OwnedNetworkBehaviour>();
            if (ownedObject != null)
            {
                ownedObject.SetOwner(owner);
                owner.AddOwnedGameObject(newGO);
            }
            else
                Debug.LogError("player parameter was given, but for a GameObject created from a prefab that is not supposed to be owned.");
        }

        /* NOTE: Always set properties before spawning object, if not there will be a delay before all clients get the values. */
        if (conn != null)
            NetworkServer.SpawnWithClientAuthority(newGO, conn);
        else if (NetworkServer.active)
            NetworkServer.Spawn(newGO);
        else
            Debug.LogWarning("NetworkServer not active");

        return newGO;
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
            if (resourceController.RemainingItems <= 0)
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

    // I belive finding by tag is a quick unity action and not neccesary to cache
    public List<PlayerController> FindPlayerControllers()
    {
        return GameObject.FindGameObjectsWithTag("Player")
            .Where(go => go.GetComponent<PlayerController>() != null)
            .Select(go => go.GetComponent<PlayerController>()).ToList();
    }

    public PlayerController FindPlayerController(string connectionID)
    {
        return FindPlayerControllers().Where(p => p.connectionId == connectionID).FirstOrDefault();
    }

    public PlayerController FindClientsOwnPlayer()
    {
        return FindPlayerControllers().Where(p => p.hasAuthority).FirstOrDefault();
    }

    public CityController FindCityController(string connectionID)
    {
        return GameObject.FindGameObjectsWithTag("PlayerCity")
            .Select(go => go.GetComponent<CityController>())
            .Where(p => p != null && p.Owner != null && p.Owner.connectionId == connectionID).FirstOrDefault();
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
