using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour
{

    public Transform groundTransform;
    public GameObject playerCityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;

    private int width = 100;
    private int height = 100;

    private int[,] tiles;


	// Use this for initialization
	private void Start ()
    {
        tiles = new int[width, height];

        AdjustGround();

        WorldBuilder worldBuilder = new WorldBuilder(width, height, 2, 10, 10);

        Vector3 p1position = new Vector3(worldBuilder.playerCoordinates[0].x, 0, worldBuilder.playerCoordinates[0].z);
        SpawnPlayerCity(p1position, "Andriod");
        Vector3 p2position = new Vector3(worldBuilder.playerCoordinates[1].x, 0, worldBuilder.playerCoordinates[1].z);
        SpawnPlayerCity(p2position, "Shrubbz");

        foreach (Coordinate coord in worldBuilder.copperNodeCoordinates)
            SpawnObject(copperNodePrefab, "CopperNode_" + coord.x + "_" + coord.z, coord.x, coord.z);

        foreach (Coordinate coord in worldBuilder.ironNodeCoordinates)
            SpawnObject(ironNodePrefab, "IronNode_" + coord.x + "_" + coord.z, coord.x, coord.z);
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void SpawnPlayerCity(Vector3 position, string playerName)
    {
        SpawnObject(playerCityPrefab, playerName+"_City", position.x, position.z);
    }

    private GameObject SpawnObject(GameObject prefab, string name, float x, float z, GameObject parent = null)
    {
        GameObject gameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
        gameObject.name = name;

        if (parent != null)
            gameObject.transform.parent = parent.transform;

        return gameObject;
    }

    private void AdjustGround()
    {
        float xPosition = (width / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles
        float zPosition = (height / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles

        groundTransform.position = new Vector3(xPosition, -0.001f, zPosition);
        groundTransform.localScale = new Vector3(width / 10, 1, height / 10);
    }



}
