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

    List<Coordinate> reservedPlayerCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method
    int playerCordIncrement = 0;

    public List<Coordinate> copperNodeCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method
    public List<Coordinate> ironNodeCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method

    public WorldBuilder(int width, int height, int reservedPlayerSpotCount, int extraCopperNodeCount, int extraIronNodeCount)
    {
        this.width = width;
        this.height = height;
        tiles = new TileType[width, height];

        /* Reserve <playerCount> number of spots for playerCities before allocating any other tiles */
        for (int i = 0; i < reservedPlayerSpotCount; i++)
            ReservePlayerCoordinate(GetRandomOpenCoordinate());

        for (int i = 0; i < extraCopperNodeCount; i++)
            AddCopperNode(GetRandomOpenCoordinate());

        for (int i = 0; i < extraIronNodeCount; i++)
            AddIronNode(GetRandomOpenCoordinate());
    }

    public Coordinate GetNextPlayerPosition()
    {
        if (playerCordIncrement >= reservedPlayerCoordinates.Count)
            return GetRandomOpenCoordinate();

        var nextPos = reservedPlayerCoordinates[playerCordIncrement];
        playerCordIncrement++;
        return nextPos;
    }

    public List<Coordinate> GetCityOrReservedCoordinates()
    {
        List<Coordinate> cities = new List<Coordinate>();

        for (int x = 0; x < tiles.GetLength(0); x++)
            for (int z = 0; z < tiles.GetLength(1); z++)
                if (tiles[x, z] == TileType.City || tiles[x, z] == TileType.CityReservation)
                    cities.Add(new Coordinate(x, z));

        return cities;
    }

    private void ReservePlayerCoordinate(Coordinate playerCityCoordinate)
    {
        reservedPlayerCoordinates.Add(playerCityCoordinate);

        AddCopperNode(GetRandomOpenCoordinateNear(playerCityCoordinate, 1, 3));
        AddIronNode(GetRandomOpenCoordinateNear(playerCityCoordinate, 1, 3));

        tiles[playerCityCoordinate.x, playerCityCoordinate.z] = TileType.CityReservation;
    }

    private void AddCopperNode(Coordinate coord)
    {
        copperNodeCoordinates.Add(coord);
        tiles[coord.x, coord.z] = TileType.Resource;
    }

    private void AddIronNode(Coordinate coord)
    {
        ironNodeCoordinates.Add(coord);
        tiles[coord.x, coord.z] = TileType.Resource;
    }

    private Coordinate GetRandomOpenCoordinate()
    {
        int attempts = 0;
        while (true)
        {
            attempts++;
            if (attempts > 1000)
                throw new Exception("Failed to find a random open coordinate, should probably rewrite the algorithm anyway");

            int x = Utils.RandomInt(0, width);
            int z = Utils.RandomInt(0, height);
            if (tiles[x, z] == TileType.Empty)
                return new Coordinate(x, z);
        }
    }

    /// <summary>
    /// Image you have two Squares, one big and one small. This method uses minDistance-1 as a little square and maxDistance as a big square.
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

        return Utils.Random(openCoords);
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
}

public enum TileType
{
    Empty, // Is defaulted to this when initing enum array
    CityReservation,
    City,
    Resource
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