using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class WorldBuilder
{
    private int width;
    private int height;

    private TileType[,] tiles;
    public TileType[,] Tiles { get { return tiles; } }
    public float[,] noiseMap;

    Stack<Coordinate> reservedPlayerCoordinates = new Stack<Coordinate>();

    public WorldBuilder(int width, int height, int maxPlayers, NoiseConfig noiseConfig)
    {
        this.width = width;
        this.height = height;
        tiles = new TileType[width, height];

        for (int i = 0; i < maxPlayers; i++)
            GeneratePlayerArea(GetRandomEmptyCoordinate());

        if (noiseConfig != null)
            GenerateResources(noiseConfig);
    }

    private void GenerateResources(NoiseConfig noiseConfig)
    {
        noiseMap = NoiseUtils.GenerateNoiseMap(width, height, noiseConfig);

        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int z = 0; z < noiseMap.GetLength(1); z++)
            {
                if (tiles[x, z] != TileType.Empty)
                    continue;
                else if (MathUtils.InRange(Settings.World_Gen_CopperNoiseRangeFrom, Settings.World_Gen_CopperNoiseRangeTo, noiseMap[x, z]))
                    SetTile(x, z, TileType.CopperNode);
                else if (MathUtils.InRange(Settings.World_Gen_IronNoiseRangeFrom, Settings.World_Gen_IronNoiseRangeTo, noiseMap[x, z]))
                    SetTile(x, z, TileType.IronNode);
                else if (MathUtils.InRange(Settings.World_Gen_FoodNoiseRangeFrom, Settings.World_Gen_FoodNoiseRangeTo, noiseMap[x, z]))
                    SetTile(x, z, TileType.FoodNode);
                //else
                //Debug.Log("Wierd noise value: " + noiseMap[x, y]);
            }
        }
    }

    public Coordinate GetNextPlayerPosition()
    {
        if (reservedPlayerCoordinates.Count == 0)
            throw new Exception("Out of reserved player coordinates. Should not allow players to join when no player coordinates is left. Someone messed up!");
        else
            return reservedPlayerCoordinates.Pop();
    }

    public List<Coordinate> GetCityOrReservedCoordinates()
    {
        List<Coordinate> cities = new List<Coordinate>();

        for (int x = 0; x < tiles.GetLength(0); x++)
            for (int z = 0; z < tiles.GetLength(1); z++)
                if (tiles[x, z] == TileType.PlayerCity || tiles[x, z] == TileType.PlayerCityReservation)
                    cities.Add(new Coordinate(x, z));

        return cities;
    }

    private void GeneratePlayerArea(Coordinate playerCityCoordinate)
    {
        reservedPlayerCoordinates.Push(playerCityCoordinate);

        SetTile(playerCityCoordinate.x, playerCityCoordinate.z, TileType.PlayerCityReservation);

        List<Coordinate> playerResourceCordsList = GetCoordinatesNear(playerCityCoordinate, Settings.World_Gen_PlayerStartingAreaResourceRadius, TileType.Empty);
        Utils.Shuffle(playerResourceCordsList);
        Stack<Coordinate> playerResourceCordsStack = new Stack<Coordinate>(playerResourceCordsList);

        for (int _ = 0; _ < Settings.World_Gen_PlayerStartingAreaCopper; _++)
            SetTile(playerResourceCordsStack.Pop(), TileType.CopperNode);
        for (int _ = 0; _ < Settings.World_Gen_PlayerStartingAreaIron; _++)
            SetTile(playerResourceCordsStack.Pop(), TileType.IronNode);
        for (int _ = 0; _ < Settings.World_Gen_PlayerStartingAreaFood; _++)
            SetTile(playerResourceCordsStack.Pop(), TileType.FoodNode);

        foreach (Coordinate cord in GetCoordinatesNear(playerCityCoordinate, Settings.World_Gen_PlayerAreaRadius, TileType.Empty))
            SetTile(cord, TileType.PlayerArea);
    }

    private void SetTile(Coordinate coord, TileType type)
    {
        SetTile(coord.x, coord.z, type);
    }

    private void SetTile(int x, int z, TileType type)
    {
        if (tiles[x, z] == TileType.Empty)
            tiles[x, z] = type;
    }

    private Coordinate GetRandomEmptyCoordinate()
    {
        int attempts = 0;
        while (true)
        {
            attempts++;
            if (attempts > 3000)
                throw new Exception("Failed to find a random open coordinate, should probably rewrite the algorithm anyway");

            int x = Utils.RandomInt(0, width);
            int z = Utils.RandomInt(0, height);
            if (tiles[x, z] == TileType.Empty)
                return new Coordinate(x, z);
        }
    }

    /// <summary>
    /// Image you have two Squares, one big and one small. This method uses (minDistance - 1) as a little square and maxDistance as a big square.
    /// It finds availible open spots by withdrawing from the big square coordinates that is in the small square.
    /// </summary>
    private Coordinate GetRandomOpenCoordinateNear(Coordinate coord, int minDistance, int maxDistance)
    {
        List<Coordinate> openCoords = new List<Coordinate>();

        List<Coordinate> ignoringCoordinates = GetCoordinatesNear(coord, minDistance - 1, TileType.Empty);

        foreach (Coordinate potentialOpenCoord in GetCoordinatesNear(coord, maxDistance, TileType.Empty))
            if (!ignoringCoordinates.Exists(c => c.x == potentialOpenCoord.x && c.z == potentialOpenCoord.z))
                openCoords.Add(potentialOpenCoord);

        //Debug.LogFormat("{0} has {1} nearby open coordinates with minDistance {2} and maxDistance {3} ", coord, openCoords.Count, minDistance, maxDistance);

        if (openCoords.Count == 0)
            throw new Exception(string.Format("Could not find an open coordinate near {0} with minDistance {1} and maxDistance {2}.", coord, minDistance, maxDistance));
        else
            return openCoords.TakeRandom();
    }

    public List<Coordinate> GetCoordinatesNear(Coordinate coord, int maxDistance, TileType? filter = null)
    {
        return GetCoordinatesNear(coord.x, coord.z, maxDistance, filter);
    }

    public List<Coordinate> GetCoordinatesNear(int nearX, int nearZ, int maxDistance, TileType? filter = null)
    {
        int minX = Math.Max(0, nearX - maxDistance);
        int maxX = Math.Min(width - 1, nearX + maxDistance);
        int minZ = Math.Max(0, nearZ - maxDistance);
        int maxZ = Math.Min(height - 1, nearZ + maxDistance);

        List<Coordinate> openCoords = new List<Coordinate>();
        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
                if (filter == null || tiles[x, z] == filter)
                    openCoords.Add(new Coordinate(x, z));

        return openCoords;
    }

    public bool IsWithinWorld(int x, int z)
    {
        return 
            x >= 0 
            && x < width 
            && z >= 0 
            && z < height;
    }
}

public enum TileType
{
    Empty, // Is defaulted to this when initing enum array
    PlayerCityReservation,
    PlayerCity,
    PlayerArea,
    CopperNode,
    IronNode,
    FoodNode
}

public class Coordinate
{
    public int x;
    public int z;

    public Coordinate(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return string.Format("Coord({0},{1})", x, z);
    }
}