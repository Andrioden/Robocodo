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
    public void Initialize(int width, int height, List<Coordinate> cityOrReservedCoordinates)
    {
        this.width = width;
        this.height = height;
        tileInfectionGameObjects = new GameObject[width, height];
        AddBigInfectionAwayFromCities(cityOrReservedCoordinates);

        WorldTickController.instance.OnTick += Tick;
    }

    [Server]
    private void AddBigInfectionAwayFromCities(List<Coordinate> cityOrReservedCoordinates)
    {
        int startSearchX = (width / 2) + Utils.RandomInt(width / -10, width / 10);
        int startSearchZ = (height / 2) + Utils.RandomInt(height / -10, height / 10);

        for (int x = startSearchX; x < width; x++)
        {
            for (int z = startSearchZ; z < height; z++)
            {
                if (!IsNearCityOrReserved(x, z, cityOrReservedCoordinates))
                {
                    IncreaseOrAddTileInfection(x, z, 1);
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
    private void IncreaseOrSpreadInfection(TileInfection ti, int increasion)
    {
        if (ti.Infection <= 0)
            return;
        else if (ti.Infection < 100)
            IncreaseOrAddTileInfection(ti.X, ti.Z, increasion);
        else // Can only spread to adjacent
        {
            List<Coordinate> coords = WorldController.instance.worldBuilder.GetCoordinatesNear(ti.X, ti.Z, Settings.World_Infection_SpreadDistance);
            Utils.Shuffle(coords);

            foreach (var coord in coords)
            {
                if (IncreaseOrAddTileInfection(coord.x, coord.z, increasion))
                    return;
            }

            // Did not managed to spread, ignore this tile in the future
            _spreadingTileInfectionIndexes.Remove(IndexOfTileInfection(ti.X, ti.Z));
        }
    }

    /// <summary>
    /// Returns true if it managed to spread
    /// </summary>
    [Server]
    public bool IncreaseOrAddTileInfection(int x, int z, int increasion)
    {
        int index = IndexOfTileInfection(x, z);
        if (index != -1)
        {
            if (tileInfections[index].Infection < 100)
                tileInfections[index] = new TileInfection(x, z, tileInfections[index].Infection + increasion);
            else
                return false;
        }
        else
        {
            tileInfections.Add(new TileInfection(x, z, increasion));
            _spreadingTileInfectionIndexes.Add(tileInfections.Count - 1);
        }

        return true;
    }

    /// <summary>
    /// Returns true if tile after this reduction has no infection left
    /// </summary>
    [Server]
    public bool DecreaseTileInfection(int x, int z, int reduction)
    {
        int index = IndexOfTileInfection(x, z);
        if (index != -1)
        {
            int newTileInfectionValue = Math.Max(0, tileInfections[index].Infection - reduction);
            tileInfections[index] = new TileInfection(x, z, newTileInfectionValue);

            _AllowAdjacentTilesToSpreadAgain(x, z);

            return newTileInfectionValue <= 0;
        }
        else // Not found
            return true;
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

    /// <summary>
    /// We have to write our own method, because the SyncListStruct messes up normal IndexOf().
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
        float cutoff = (100 - ti.Infection) / 100.0f;

        if (cutoff == 0)
            renderer.material = fullInfectionMaterial; //When full infection, use shared material that never changes to increase performance.
        else
        {
            renderer.material = partialInfectionMaterial;
            renderer.material.SetFloat("_Cutoff", cutoff);
        }
    }

    // Has to be inside the class using it, also cant directly use SyncListStruct<TileInfection>. gg!
    public class SyncListTileInfection : SyncListStruct<TileInfection> { }
}

public struct TileInfection
{
    public int X;
    public int Z;
    public int Infection;

    public TileInfection(int x, int z, int infection)
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