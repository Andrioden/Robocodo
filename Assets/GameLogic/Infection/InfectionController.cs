using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;

public class InfectionController : NetworkBehaviour
{

    public GameObject tileInfectionPrefab;

    private GameObject tileInfectionsParent;
    private GameObject[,] tileInfectionGameObjects;

    [SyncVar]
    private int width;
    [SyncVar]
    private int height;

    private SyncListTileInfection tileInfections = new SyncListTileInfection();

    /// <summary>
    /// Guaranteed to run after SyncList is synced (https://docs.unity3d.com/ScriptReference/Networking.NetworkBehaviour.OnStartClient.html)
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isClient)
            tileInfectionGameObjects = new GameObject[width, height];

        tileInfectionsParent = new GameObject("TileInfections");
        tileInfections.Callback = SpawnOrUpdateTileInfectionLOL;

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
                    SetOrUpdateInfection(x, z, 44);
                    return;
                }
            }
        }
    }

    [Server]
    private void SetOrUpdateInfection(int x, int z, int infection)
    {
        TileInfection? ti = tileInfections.Where(t => t.x == x && t.z == z).Cast<TileInfection?>().FirstOrDefault();
        if (ti.HasValue)
        {
            TileInfection tiValue = ti.Value;
            tiValue.infection = infection;
        }
        else
            tileInfections.Add(new TileInfection(x, z, infection));
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
    private void SpawnOrUpdateTileInfectionLOL(SyncListTileInfection.Operation op, int index)
    {
        Debug.Log("LOL JOIN " + index);
        SpawnOrUpdateTileInfection(tileInfections[index]);
    }

    [Client]
    private void SpawnOrUpdateTileInfection(TileInfection ti)
    {
        if (tileInfectionGameObjects[ti.x, ti.z] == null)
        {
            GameObject tileInfectionGO = (GameObject)Instantiate(tileInfectionPrefab, tileInfectionsParent.transform);
            tileInfectionGO.transform.position = new Vector3(ti.x, tileInfectionGO.transform.position.y, ti.z);
            tileInfectionGameObjects[ti.x, ti.z] = tileInfectionGO;
        }

        UpdateTileInfectionTransparency(tileInfectionGameObjects[ti.x, ti.z], ti);
    }

    [Client]
    private void UpdateTileInfectionTransparency(GameObject go, TileInfection ti)
    {
        Material goMaterial = go.GetComponent<Renderer>().material;
        goMaterial.color = new Color(goMaterial.color.r, goMaterial.color.g, goMaterial.color.b, ti.infection / 100.0f);
    }

    // Has to be inside the class using it, also cant directly use SyncListStruct<TileInfection>. gg!
    public class SyncListTileInfection : SyncListStruct<TileInfection>{ }
}

public struct TileInfection
{
    public int x;
    public int z;
    public int infection; // 0 - 100, where 100 is max

    public TileInfection(int x, int z, int infection)
    {
        this.x = x;
        this.z = z;
        this.infection = infection;
    }
}