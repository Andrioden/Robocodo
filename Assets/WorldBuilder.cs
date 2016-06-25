using System.Collections;
using System.Collections.Generic;
using System;

public class WorldBuilder
{
    private int width;
    private int height;


    public bool[,] tiles;

    List<Coordinate> reservedPlayerCoordinates = new List<Coordinate>();
    int playerCordIncrement = 0;

    public List<Coordinate> copperNodeCoordinates = new List<Coordinate>();
    public List<Coordinate> ironNodeCoordinates = new List<Coordinate>();

    public WorldBuilder(int width, int height, int playerCount, int copperNodeCount, int ironNodeCount)
    {
        this.width = width;
        this.height = height;
        tiles = new bool[width, height];

        /* Reserve <playerCount> number of spots for playerCities before allocating any other tiles */
        for (int i = 0; i < playerCount; i++)
            reservedPlayerCoordinates.Add(GetRandomOpenCoordinate());

        for (int i = 0; i < copperNodeCount; i++)
            copperNodeCoordinates.Add(GetRandomOpenCoordinate());

        for (int i = 0; i < ironNodeCount; i++)
            ironNodeCoordinates.Add(GetRandomOpenCoordinate());
    }

    public Coordinate GetRandomOpenCoordinate()
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
            {
                tiles[x, z] = false;
                return new Coordinate(x, z);
            }
        }
    }

    public Coordinate GetNextPlayerPosition()
    {
        if (playerCordIncrement > reservedPlayerCoordinates.Count)
            return GetRandomOpenCoordinate();

        var nextPos = reservedPlayerCoordinates[playerCordIncrement];
        playerCordIncrement++;
        return nextPos;
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
}