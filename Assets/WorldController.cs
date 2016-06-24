﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class WorldController : NetworkBehaviour
{

    public Transform groundTransform;
    public GameObject playerCityPrefab;
    public GameObject copperNodePrefab;
    public GameObject ironNodePrefab;
    public GameObject harvesterRobotPrefab;

    private int width = 100;
    private int height = 100;

    private int[,] tiles;


	// Use this for initialization
	private void Start ()
    {
        tiles = new int[width, height];
        AdjustGround();

        if (isServer)
            BuildWorld();
    }

    private void BuildWorld()
    {
        WorldBuilder worldBuilder = new WorldBuilder(width, height, 2, 10, 10);

        Vector3 p1position = new Vector3(worldBuilder.playerCoordinates[0].x, 0, worldBuilder.playerCoordinates[0].z);
        SpawnPlayerCity(p1position, "Andriod");
        Vector3 p2position = new Vector3(worldBuilder.playerCoordinates[1].x, 0, worldBuilder.playerCoordinates[1].z);
        SpawnPlayerCity(p2position, "Shrubbz");

        SpawnObject(harvesterRobotPrefab, "Robot1", p1position.x, p1position.z);
        SpawnObject(harvesterRobotPrefab, "Robot2", p1position.x, p1position.z);

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
        GameObject newGameObject = (GameObject)Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
        newGameObject.name = name;

        if (parent != null)
            newGameObject.transform.parent = parent.transform;

        NetworkServer.Spawn(newGameObject);

        return newGameObject;
    }

    private void AdjustGround()
    {
        float xPosition = (width / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles
        float zPosition = (height / 2) - 0.5f; // Hack: The -0.5f is an offset we have to set to align the ground to the tiles

        groundTransform.position = new Vector3(xPosition, -0.001f, zPosition);
        groundTransform.localScale = new Vector3(width / 10, 1, height / 10);
    }



}
