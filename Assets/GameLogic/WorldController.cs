using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class WorldController : NetworkBehaviour
{
    public GameObject groundPrefab;
    public GameObject playerCityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;
    public GameObject harvesterRobotPrefab;
    public GameObject combatRobotPrefab;

    private WorldBuilder worldBuilder;
    [SyncVar]
    private int width;
    public int Width { get { return width; } }
    [SyncVar]
    private int height;
    public int Height { get { return height; } }

    private List<ResourceController> resourceControllers = new List<ResourceController>();

    private Transform worldParent;

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
    }

    // Use this for initialization
    private void Start()
    {
        SpawnAndAdjustGround();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void BuildWorld(int width, int height)
    {
        this.width = width;
        this.height = height;

        worldBuilder = new WorldBuilder(width, height, 10, 10, 10);

        foreach (Coordinate coord in worldBuilder.copperNodeCoordinates)
            SpawnResourceNode(copperNodePrefab, coord.x, coord.z);

        foreach (Coordinate coord in worldBuilder.ironNodeCoordinates)
            SpawnResourceNode(ironNodePrefab, coord.x, coord.z);
    }

    public void BuildWorldDemoWorld(int width, int height, Transform demoWorldParent)
    {
        worldParent = demoWorldParent;
        BuildWorld(width, height);
        SpawnPlayer(null, 0);
        SpawnAndAdjustGround();
    }

    [Server]
    public void SpawnPlayer(NetworkConnection conn, short playerControllerId)
    {
        var playerPos = worldBuilder.GetNextPlayerPosition();
        GameObject newGameObject = (GameObject)Instantiate(playerCityPrefab, new Vector3(playerPos.x, 0, playerPos.z), Quaternion.identity);
        PlayerCityController newPlayerCity = newGameObject.GetComponent<PlayerCityController>();

        if (worldParent != null)
            newGameObject.transform.parent = worldParent;

        if (NetworkServer.active)
            NetworkServer.AddPlayerForConnection(conn, newGameObject, playerControllerId);
        else
            Debug.LogError("Network server is not active!");

        for (int i = 0; i < Settings.Player_AmountOfStartingHarvesterRobots; i++)
        {
            GameObject harvesterGO = SpawnHarvesterRobotWithClientAuthority(conn, playerPos.x, playerPos.z);
            newPlayerCity.AddOwnedGameObject(harvesterGO);
        }

        GameObject combatRobotGO = SpawnCombatRobotWithClientAuthority(conn, playerPos.x, playerPos.z);
        newPlayerCity.AddOwnedGameObject(combatRobotGO);
    }

    [Server]
    public GameObject SpawnCombatRobotWithClientAuthority(NetworkConnection conn, int x, int z)
    {
        return SpawnObjectWithClientAuthority(conn, combatRobotPrefab, x, z);
    }

    [Server]
    public GameObject SpawnHarvesterRobotWithClientAuthority(NetworkConnection conn, int x, int z)
    {
        return SpawnObjectWithClientAuthority(conn, harvesterRobotPrefab, x, z);
    }

    [Server]
    private GameObject SpawnObjectWithClientAuthority(NetworkConnection conn, GameObject prefab, int x, int z)
    {
        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        if (worldParent != null)
            newGameObject.transform.parent = worldParent;

        if (NetworkServer.active)
        {
            NetworkServer.SpawnWithClientAuthority(newGameObject, conn);

            var robot = newGameObject.GetComponent<RobotController>();
            if (robot != null)
                robot.owner = conn.connectionId.ToString();
        }
        else
            Debug.LogError("Network server is not active!");

        return newGameObject;
    }

    [Server]
    private void SpawnResourceNode(GameObject prefab, int x, int z)
    {
        ResourceController resourceController = SpawnObject(prefab, x, z).GetComponent<ResourceController>();

        if (prefab == ironNodePrefab)
            resourceController.resourceType = IronItem.SerializedType;
        else if (prefab == copperNodePrefab)
            resourceController.resourceType = CopperItem.SerializedType;
        else
            throw new Exception("Not added support for given resource prefab. Get coding!");

        resourceControllers.Add(resourceController);
    }

    [Server]
    public GameObject SpawnObject(GameObject prefab, int x, int z)
    {
        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);

        if (worldParent != null)
            newGameObject.transform.parent = worldParent;

        if (NetworkServer.active)
            NetworkServer.Spawn(newGameObject);
        else
            Debug.LogError("Network server is not active!");

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
            Debug.LogFormat("Server: HARVESTED! Resource node at {0},{1} has {2} items left", x, z, resourceController.GetRemainingResourceItems());
            if (resourceController.GetRemainingResourceItems() <= 0)
            {
                resourceControllers.Remove(resourceController);
                Destroy(resourceController.gameObject);
            }
            return true;
        }
        else
        {
            Debug.LogFormat("Server: No resource found at {0},{1}", x, z);
            return false;
        }
    }

    public void SpawnAndAdjustGround()
    {
        float xPosition = (width / 2f) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles
        float zPosition = (height / 2f) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles

        GameObject groundGameObject = (GameObject)Instantiate(groundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        groundGameObject.name = "Ground_NotNetwork";

        if (worldParent != null)
            groundGameObject.transform.parent = worldParent;

        groundGameObject.transform.localScale = new Vector3(width / 10f, 1, height / 10f);
        groundGameObject.transform.position = new Vector3(xPosition, -0.001f, zPosition);

        groundGameObject.GetComponent<TextureTilingController>().RescaleTileTexture();
    }
}
