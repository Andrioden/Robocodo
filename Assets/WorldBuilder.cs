using System.Collections;
using System.Collections.Generic;
using System;

public class WorldBuilder
{
    private int width;
    private int height;

    int playerCordIncrement = 0;

    public bool[,] tiles;
    List<Coordinate> playerCoordinates = new List<Coordinate>();
    public List<Coordinate> copperNodeCoordinates = new List<Coordinate>();
    public List<Coordinate> ironNodeCoordinates = new List<Coordinate>();

    public bool Build(int width, int height, int playerCount, int copperNodeCount, int ironNodeCount)
    {
        this.width = width;
        this.height = height;
        tiles = new bool[width, height];

        /* Reserve <playerCount> number of spots for playerCities before allocating any other tiles */
        for (int i = 0; i < playerCount; i++)
            playerCoordinates.Add(GetRandomOpenCoordinate());

        /* Reserve spots for world objects */
        for (int i = 0; i < copperNodeCount; i++)
            copperNodeCoordinates.Add(GetRandomOpenCoordinate());

        for (int i = 0; i < ironNodeCount; i++)
            ironNodeCoordinates.Add(GetRandomOpenCoordinate());

        return true;
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
        if (playerCordIncrement > playerCoordinates.Count)
            return GetRandomOpenCoordinate();

        var nextPos = playerCoordinates[playerCordIncrement];
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