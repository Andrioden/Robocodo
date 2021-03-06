﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System;

public class InfectionManager : NetworkBehaviour
{

    public GameObject tileInfectionPrefab;
    public Material fullInfectionMaterial;
    public Material partialInfectionMaterial;

    private GameObject parent;
    private GameObject[,] tileInfectionGameObjects;
    private float[,] maxInfectionNoiseMap;

    [SyncVar]
    private int width;
    [SyncVar]
    private int height;

    private SyncListTileInfection tileInfections = new SyncListTileInfection(); // Should never have its elements removed, only set to 0 if cleaned, because of index cache below
    public SyncListTileInfection TileInfections { get { return tileInfections; } }
    private List<int> _spreadingTileInfectionIndexes = new List<int>();

    public static InfectionManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
        {
            Debug.LogError("Tried to create another instance of " + GetType() + ". Destroying.");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Guaranteed to run after SyncList is synced (https://docs.unity3d.com/ScriptReference/Networking.NetworkBehaviour.OnStartClient.html)
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isClient)
            tileInfectionGameObjects = new GameObject[width, height];

        parent = new GameObject("TileInfections");
        parent.transform.SetParentToGO("ClientGameObjects");

        tileInfections.Callback = SpawnOrUpdateTileInfection;

        if (isClient)
            SpawnOrUpdateTileInfections();
    }

    [Server]
    public void Initialize(int width, int height)
    {
        this.width = width;
        this.height = height;
        tileInfectionGameObjects = new GameObject[width, height];
        maxInfectionNoiseMap = NoiseUtils.GenerateNoiseMap(width, height, 0.12f);

        WorldTickController.instance.OnTick += Tick;
    }

    [Server]
    public void AddBigInfectionAwayFromCities(List<Coordinate> cityOrReservedCoordinates)
    {
        int startSearchX = (width / 2) + Utils.RandomInt(width / -10, width / 10);
        int startSearchZ = (height / 2) + Utils.RandomInt(height / -10, height / 10);

        for (int x = startSearchX; x < width; x++)
        {
            for (int z = startSearchZ; z < height; z++)
            {
                if (!IsNearCityOrReserved(x, z, cityOrReservedCoordinates))
                {
                    IncreaseTileInfection(x, z, 1);
                    return;
                }
            }
        }
    }

    [Server]
    private void Tick()
    {
        for (int _indexIndex = 0; _indexIndex < _spreadingTileInfectionIndexes.Count; _indexIndex++)
            IncreaseOrSpreadInfection(tileInfections[_spreadingTileInfectionIndexes[_indexIndex]], Settings.World_Infection_GrowthPerTickPerTile);
    }

    [Server]
    private void IncreaseOrSpreadInfection(TileInfection ti, double increased)
    {
        float maxInfection = maxInfectionNoiseMap[ti.X, ti.Z] * 100;

        if (ti.Infection <= 0)
            return;
        else if (ti.Infection >= maxInfection)
            SpreadInfection(ti, increased);
        else
            IncreaseTileInfection(ti.X, ti.Z, increased);
    }

    [Server]
    private void SpreadInfection(TileInfection fromTile, double increased)
    {
        List<Coordinate> coords = WorldController.instance.worldBuilder.GetCoordinatesNear(fromTile.X, fromTile.Z, Settings.World_Infection_SpreadDistance);
        Utils.Shuffle(coords);

        foreach (var coord in coords)
            if (IncreaseTileInfection(coord.x, coord.z, increased))
                return;

        // Did not managed to spread, ignore this tile in the future
        _spreadingTileInfectionIndexes.Remove(IndexOfTileInfection(fromTile.X, fromTile.Z));
    }

    /// <summary>
    /// Returns true if it managed to spread
    /// </summary>
    [Server]
    public bool IncreaseTileInfection(int x, int z, double increase)
    {
        float maxInfection = maxInfectionNoiseMap[x, z] * 100;

        int index = IndexOfTileInfection(x, z);
        if (index != -1)
        {
            if (tileInfections[index].Infection < maxInfection)
            {
                double newInfection = Math.Min(maxInfection, tileInfections[index].Infection + increase);
                tileInfections[index] = new TileInfection(x, z, newInfection);
            }
            else
                return false;
        }
        else
            AddNewTileInfection(x, z, increase);

        return true;
    }

    private void AddNewTileInfection(int x, int z, double increasion)
    {
        tileInfections.Add(new TileInfection(x, z, increasion));
        _spreadingTileInfectionIndexes.Add(tileInfections.Count - 1);
    }

    /// <summary>
    /// Returns the remaining tile infection value after decreasing
    /// </summary>
    [Server]
    public double DecreaseTileInfection(int x, int z, PlayerController player , int maxReduction)
    {
        int index = IndexOfTileInfection(x, z);
        if (index != -1)
        {
            double newTileInfectionValue = Math.Max(0, tileInfections[index].Infection - maxReduction);
            double reduction = tileInfections[index].Infection - newTileInfectionValue;
            tileInfections[index] = new TileInfection(x, z, newTileInfectionValue);

            _AllowAdjacentTilesToSpreadAgain(x, z);

            UpdatePlayerContribution(player, tileInfections[index], reduction);

            return newTileInfectionValue;
        }
        else // Not found
            return 0;
    }

    [Server]
    private void _AllowAdjacentTilesToSpreadAgain(int x, int z)
    {
        List<Coordinate> coords = WorldController.instance.worldBuilder.GetCoordinatesNear(x, z, Settings.World_Infection_SpreadDistance);
        foreach(Coordinate cord in coords)
        {
            int index = IndexOfTileInfection(cord.x, cord.z);
            if (index == -1)
                continue;

            if (!_spreadingTileInfectionIndexes.Exists(i => i == index))
                _spreadingTileInfectionIndexes.Add(index);
        }
    }

    [Server]
    private void UpdatePlayerContribution(PlayerController player, TileInfection ti, double reduction)
    {
        double distance = MathUtils.Distance(player.City.X, player.City.Z, ti.X, ti.Z);
        double contribution = Math.Sqrt(distance) * reduction;
        player.infectionContribution += contribution;
    }

    [Server]
    public double GetTileInfection(int x, int z)
    {
        int index = IndexOfTileInfection(x, z);
        if (index != -1)
            return tileInfections[index].Infection;
        else
            return 0;
    }

    /// <summary>
    /// We have to write our own method, because the SyncListStruct messes up normal IndexOf(). 
    /// Also keep in mind TileInfection is an struct, so an reference of a struct reference cant normaly be passed around.
    /// </summary>
    [Server]
    private int IndexOfTileInfection(int x, int z)
    {
        for (int i = 0; i < tileInfections.Count; i++)
        {
            if (tileInfections[i].X == x && tileInfections[i].Z == z)
                return i;
        }
        return -1;
    }

    [Server]
    private bool IsNearCityOrReserved(int x, int z, List<Coordinate> cityOrReservedCoordinates)
    {
        //int minDistanceFromCity = 0;
        return false;
    }

    [Client]
    private void SpawnOrUpdateTileInfections()
    {
        tileInfections.ToList().ForEach(ti => SpawnOrUpdateTileInfection(ti));
    }

    [Client]
    private void SpawnOrUpdateTileInfection(SyncListTileInfection.Operation op, int index)
    {
        SpawnOrUpdateTileInfection(tileInfections[index]);
    }

    [Client]
    private void SpawnOrUpdateTileInfection(TileInfection ti)
    {
        //Debug.Log("Changed infection at " + ti);
        if (tileInfectionGameObjects[ti.X, ti.Z] == null)
        {
            GameObject tileInfectionGO = (GameObject)Instantiate(tileInfectionPrefab, parent.transform);
            tileInfectionGO.transform.position = new Vector3(ti.X, tileInfectionGO.transform.position.y, ti.Z);
            tileInfectionGameObjects[ti.X, ti.Z] = tileInfectionGO;
        }

        if (ti.Infection <= 0)
            Destroy(tileInfectionGameObjects[ti.X, ti.Z]);
        else
            UpdateTileInfectionTransparency(tileInfectionGameObjects[ti.X, ti.Z], ti);
    }

    [Client]
    private void UpdateTileInfectionTransparency(GameObject go, TileInfection ti)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        double cutoff = (100 - ti.Infection) / 100;

        if (cutoff <= 0)
            renderer.material = fullInfectionMaterial; //When full infection, use shared material that never changes to increase performance.
        else
        {
            renderer.material = partialInfectionMaterial;
            renderer.material.SetFloat("_Cutoff", (float)cutoff);
        }
    }

    // Has to be inside the class using it, also cant directly use SyncListStruct<TileInfection>. gg!
    public class SyncListTileInfection : SyncListStruct<TileInfection> { }
}

public struct TileInfection
{
    public int X;
    public int Z;
    public double Infection;

    public TileInfection(int x, int z, double infection)
    {
        X = x;
        Z = z;
        Infection = infection;
    }

    public override string ToString()
    {
        return string.Format("TI ({0},{1}).{2}", X, Z, Infection);
    }
}