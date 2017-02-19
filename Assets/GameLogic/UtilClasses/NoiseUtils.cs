using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class NoiseUtils
{


    /// <summary>
    /// Read description on other method. TODO: Se if summary can inherit from other method
    /// </summary>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseConfig config)
    {
        return GenerateNoiseMap(mapWidth, mapHeight, config.Scale, config.Octaves, config.Persistance, config.Lacunarity);
    }

    /// <summary>
    /// Noise generator from https://github.com/SebLague/Procedural-Landmass-Generation/blob/master/Proc%20Gen%20E04/Assets/Scripts/Noise.cs
    /// </summary>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale">Zooms in and out. Should be left close to 1 most of the time.</param>
    /// <param name="octaves">Intesity of the tops and bottoms (i think)</param>
    /// <param name="persistance">Between 0 and 1. A lower number makes it less detailed. Se https://youtu.be/MRNFcywkUSA?t=353 </param>
    /// <param name="lacunarity">Seems to skew the numbers a bit, dunno. Se https://youtu.be/MRNFcywkUSA?t=353 </param>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale = 0.91f, int octaves = 4, float persistance = 0.1f, float lacunarity = 2.0f)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        int seed = Utils.RandomInt(0, 999999999);
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + 0.01f;
            float offsetY = prng.Next(-100000, 100000) - 0.01f;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var normalizeMode = NormalizeMode.Global;

                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }

    public enum NormalizeMode { Local, Global };

    //public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale = 1.01f, int octaves = 4, float persistance = 0.5f, float lacunarity = 2.0f)
    //{
    //    int seed = Utils.RandomInt(0, 999999999);
    //    float[,] noiseMap = new float[mapWidth, mapHeight];

    //    System.Random prng = new System.Random(seed);
    //    Vector2[] octaveOffsets = new Vector2[octaves];
    //    for (int i = 0; i < octaves; i++)
    //    {
    //        float offsetX = prng.Next(-100000, 100000) + 0.01f;
    //        float offsetY = prng.Next(-100000, 100000) + 0.01f;
    //        octaveOffsets[i] = new Vector2(offsetX, offsetY);
    //    }

    //    if (scale <= 0)
    //    {
    //        scale = 0.0001f;
    //    }

    //    float maxNoiseHeight = float.MinValue;
    //    float minNoiseHeight = float.MaxValue;

    //    float halfWidth = mapWidth / 2f;
    //    float halfHeight = mapHeight / 2f;


    //    for (int y = 0; y < mapHeight; y++)
    //    {
    //        for (int x = 0; x < mapWidth; x++)
    //        {

    //            float amplitude = 1;
    //            float frequency = 1;
    //            float noiseHeight = 0;

    //            for (int i = 0; i < octaves; i++)
    //            {
    //                float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
    //                float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

    //                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
    //                noiseHeight += perlinValue * amplitude;

    //                amplitude *= persistance;
    //                frequency *= lacunarity;
    //            }

    //            if (noiseHeight > maxNoiseHeight)
    //            {
    //                maxNoiseHeight = noiseHeight;
    //            }
    //            else if (noiseHeight < minNoiseHeight)
    //            {
    //                minNoiseHeight = noiseHeight;
    //            }
    //            noiseMap[x, y] = noiseHeight;
    //        }
    //    }

    //    for (int y = 0; y < mapHeight; y++)
    //    {
    //        for (int x = 0; x < mapWidth; x++)
    //        {
    //            noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
    //        }
    //    }

    //    return noiseMap;
    //}

    //public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale = 1.0f)
    //{
    //    float seed = Utils.RandomFloat(-1, 1);
    //    float[,] noiseMap = new float[mapWidth, mapHeight];

    //    if (scale <= 0)
    //    {
    //        scale = 0.0001f;
    //        Debug.LogError("Scale input was less than or equal to zero.");
    //    }

    //    for (int y = 0; y < mapHeight; y++)
    //    {
    //        for (int x = 0; x < mapWidth; x++)
    //        {
    //            float sampleX = ((float)x / (float)mapWidth); //Casting is important
    //            float sampleY = ((float)y / (float)mapHeight); //Casting is important

    //            float perlinValue = Mathf.PerlinNoise(sampleX + seed, sampleY + seed);
    //            noiseMap[x, y] = perlinValue * scale;
    //        }
    //    }

    //    return noiseMap;
    //}

    //public static float[,] GenerateNoiseMap(int width, int height)
    //{
    //    float[,] noiseMap = new float[width, height];

    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int z = 0; z < height; z++)
    //        {
    //            noiseMap[x, z] = Mathf.PerlinNoise(x, z);
    //        }
    //    }

    //    return noiseMap;
    //}

}

public class NoiseConfig
{
    public float Scale;
    public int Octaves;
    public float Persistance;
    public float Lacunarity;
}