using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WorldController : NetworkBehaviour
{

    public GameObject groundPrefab;
    public GameObject playerCityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;
    public GameObject harvesterRobotPrefab;

    private WorldBuilder worldBuilder;
    [SyncVar]
    private int width;
    [SyncVar]
    private int height;

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

    [Server]
    public void BuildWorld(int width, int height)
    {
        this.width = width;
        this.height = height;

        worldBuilder = new WorldBuilder(width, height, 10, 10, 10);

        foreach (Coordinate coord in worldBuilder.copperNodeCoordinates)
            SpawnObject(copperNodePrefab, coord.x, coord.z);

        foreach (Coordinate coord in worldBuilder.ironNodeCoordinates)
            SpawnObject(ironNodePrefab, coord.x, coord.z);
    }

    [Server]
    public void SpawnPlayerCity(NetworkConnection conn, short playerControllerId)
    {
        var nextPos = worldBuilder.GetNextPlayerPosition();
        GameObject newGameObject = (GameObject)Instantiate(playerCityPrefab, new Vector3(nextPos.x, 0, nextPos.z), Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, newGameObject, playerControllerId);            
    }

    [Server]
    public void SpawnHarvesterWithClientAuthority(NetworkConnection conn, int x, int z)
    {
        SpawnObjectWithClientAuthority(conn, harvesterRobotPrefab, x, z);
    }

    [Server]
    public GameObject SpawnObjectWithClientAuthority(NetworkConnection conn, GameObject prefab, int x, int z)
    {
        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
        NetworkServer.SpawnWithClientAuthority(newGameObject, conn);

        return newGameObject;
    }

    public GameObject SpawnObject(GameObject prefab, int x, int z)
    {
        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
        NetworkServer.Spawn(newGameObject);

        return newGameObject;
    }

    private void SpawnAndAdjustGround()
    {
        Debug.Log("Spawning and adjusting ground");

        float xPosition = (width / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles
        float zPosition = (height / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles

        GameObject groundGameObject = (GameObject)Instantiate(groundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        groundGameObject.name = "Ground_NotNetwork";

        groundGameObject.transform.position = new Vector3(xPosition, -0.001f, zPosition);
        groundGameObject.transform.localScale = new Vector3(width / 10, 1, height / 10);
    }
}
