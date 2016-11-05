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
    public GameObject harvesterRobotPrefab;
    public GameObject combatRobotPrefab;
    public GameObject transporterRobotPrefab;

    private GameObject groundGameObject;

    private WorldBuilder worldBuilder;
    private PlayerColorManager playerColorManager = new PlayerColorManager();

    [SyncVar]
    private int width;
    public int Width { get { return width; } }
    [SyncVar]
    private int height;
    public int Height { get { return height; } }

    private List<ResourceController> resourceControllers = new List<ResourceController>();

    private Transform worldParent;
    private bool classIsUsedAsDemo = false;

    public static WorldController instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to created another instance of " + GetType() + ". Destroying.");
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

    // Update is called once per frame
    private void Update()
    {

    }

    private void OnDestroy()
    {
        Destroy(groundGameObject);
    }

    public void BuildWorld(int width, int height, int matchSize)
    {
        this.width = width;
        this.height = height;

        worldBuilder = new WorldBuilder(width, height, matchSize, 10, 10);

        foreach (Coordinate coord in worldBuilder.copperNodeCoordinates)
            SpawnResourceNode(copperNodePrefab, coord.x, coord.z);

        foreach (Coordinate coord in worldBuilder.ironNodeCoordinates)
            SpawnResourceNode(ironNodePrefab, coord.x, coord.z);
    }

    public void BuildWorldDemoWorld(int width, int height, Transform demoWorldParent)
    {
        classIsUsedAsDemo = true;
        worldParent = demoWorldParent;
        BuildWorld(width, height, 10);
        SpawnPlayer(null, 0);
        SpawnAndAdjustGround();
    }

    // [Server] enforced with inline code check
    public GameObject SpawnPlayer(NetworkConnection conn, short playerControllerId)
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
            NetworkServer.AddPlayerForConnection(conn, playerCityGameObject, playerControllerId);

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
    public GameObject SpawnResourceNode(GameObject prefab, int x, int z)
    {
        if (!IsServerOrDemo())
            return null;

        GameObject resurceGameObject = SpawnObject(prefab, x, z);
        ResourceController resourceController = resurceGameObject.GetComponent<ResourceController>();

        if (prefab == ironNodePrefab)
            resourceController.resourceType = IronItem.SerializedType;
        else if (prefab == copperNodePrefab)
            resourceController.resourceType = CopperItem.SerializedType;
        else
            throw new Exception("Not added support for given resource prefab. Get coding!");

        resourceControllers.Add(resourceController);

        return resurceGameObject;
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
    public bool HarvestFromNode(string resourceType, float x, float z)
    {
        ResourceController resourceController = resourceControllers.Find(r => r.resourceType == resourceType && r.transform.position.x == x && r.transform.position.z == z);
        if (resourceController != null)
        {
            resourceController.HarvestOneResourceItem();
            //Debug.LogFormat("Server: HARVESTED! Resource node at {0},{1} has {2} items left", x, z, resourceController.GetRemainingResourceItems());
            if (resourceController.GetRemainingResourceItems() <= 0)
            {
                resourceControllers.Remove(resourceController);
                Destroy(resourceController.gameObject);
            }
            return true;
        }
        else
        {
            //Debug.LogFormat("Server: No resource found at {0},{1}", x, z);
            return false;
        }
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
        // I belive finding by tag is a quick unity action
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
