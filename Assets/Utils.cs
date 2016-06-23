using System;
using System.Collections;
using System.Collections.Generic;

public static class Utils
{
    // Reusing a static random to achive uniformity (ref: http://stackoverflow.com/questions/2019417/access-random-item-in-list)
    // The point in my (André) understanding is that we create a random instance and uses the seed to get a series of random numbers, which is more uniform (spread out) than recreating the random class instance
    private static Random rnd = new Random();

    public static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T Random<T>(List<T> list)
    {
        int r = rnd.Next(list.Count);
        return list[r];
    }

    public static int RandomInt(int min, int max)
    {
        return rnd.Next(max - min) + min;
    }

    public static double RandomDouble(double min, double max)
    {
        return rnd.NextDouble() * (max - min) + min;
    }

    public static bool PercentageRoll(double percentage)
    {
        return RandomDouble(0.0, 100.0) <= percentage;
    }

}
