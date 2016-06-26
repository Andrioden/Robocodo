using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class WorldBuilder
{
    private int width;
    private int height;


    public bool[,] tiles;

    List<Coordinate> reservedPlayerCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method
    int playerCordIncrement = 0;

    public List<Coordinate> copperNodeCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method
    public List<Coordinate> ironNodeCoordinates = new List<Coordinate>(); // Should never be manipulated directly, only through the designated method

    public WorldBuilder(int width, int height, int reservedPlayerCount, int extraCopperNodeCount, int extraIronNodeCount)
    {
        this.width = width;
        this.height = height;
        tiles = new bool[width, height];

        /* Reserve <playerCount> number of spots for playerCities before allocating any other tiles */
        for (int i = 0; i < reservedPlayerCount; i++)
            ReservePlayerCoordinate(GetRandomOpenCoordinate());

        for (int i = 0; i < extraCopperNodeCount; i++)
            AddCopperNode(GetRandomOpenCoordinate());

        for (int i = 0; i < extraIronNodeCount; i++)
            AddIronNode(GetRandomOpenCoordinate());
    }

    public Coordinate GetNextPlayerPosition()
    {
        if (playerCordIncrement > reservedPlayerCoordinates.Count)
            return GetRandomOpenCoordinate();

        var nextPos = reservedPlayerCoordinates[playerCordIncrement];
        playerCordIncrement++;
        return nextPos;
    }

    private void ReservePlayerCoordinate(Coordinate playerCityCoordinate)
    {
        reservedPlayerCoordinates.Add(playerCityCoordinate);

        AddCopperNode(GetRandomOpenCoordinateNear(playerCityCoordinate, 1, 3));
        AddIronNode(GetRandomOpenCoordinateNear(playerCityCoordinate, 1, 3));

        tiles[playerCityCoordinate.x, playerCityCoordinate.z] = false;
    }

    private void AddCopperNode(Coordinate coord)
    {
        copperNodeCoordinates.Add(coord);
        tiles[coord.x, coord.z] = false;
    }

    private void AddIronNode(Coordinate coord)
    {
        ironNodeCoordinates.Add(coord);
        tiles[coord.x, coord.z] = false;
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
            if (!tiles[x, z])
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

        List<Coordinate> ignoringCoordinates = GetOpenCoordinatesNear(coord, minDistance-1);

        foreach (Coordinate potentialOpenCoord in GetOpenCoordinatesNear(coord, maxDistance))
            if (!ignoringCoordinates.Exists(c => c.x == potentialOpenCoord.x && c.z == potentialOpenCoord.z))
                openCoords.Add(potentialOpenCoord);

        //Debug.LogFormat("{0} has {1} nearby open coordinates with minDistance {2} and maxDistance {3} ", coord, openCoords.Count, minDistance, maxDistance);

        if (openCoords.Count == 0)
            throw new Exception(string.Format("Could not find an open coordinate near {0} with minDistance {1} and maxDistance {2}.", coord, minDistance, maxDistance));

        return Utils.Random(openCoords);
    }

    private List<Coordinate> GetOpenCoordinatesNear(Coordinate coord, int maxDistance)
    {
        int minX = Math.Max(0, coord.x - maxDistance);
        int maxX = Math.Min(width-1, coord.x + maxDistance);
        int minZ = Math.Max(0, coord.z - maxDistance);
        int maxZ = Math.Min(height-1, coord.z + maxDistance);

        List<Coordinate> openCoords = new List<Coordinate>();
        for (int x = minX; x <= maxX; x++)
            for (int z = minZ; z <= maxZ; z++)
                if (!tiles[x, z])
                    openCoords.Add(new Coordinate(x, z));

        return openCoords;
    }
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