﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public static class Utils
{
    // Reusing a static random to achive uniformity (ref: http://stackoverflow.com/questions/2019417/access-random-item-in-list)
    // The point in my (André) understanding is that we create a random instance and uses the seed to get a series of random numbers, which is more uniform (spread out) than recreating the random class instance
    public static Random rnd = new Random();

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

    public static int RandomInt(int min, int max)
    {
        return rnd.Next(max - min) + min;
    }

    public static double RandomDouble(double min, double max)
    {
        return rnd.NextDouble() * (max - min) + min;
    }

    public static float RandomFloat(float min, float max)
    {
        return (float)rnd.NextDouble() * (max - min) + min;
    }

    public static bool PercentageRoll(float percentage)
    {
        return PercentageRoll((double)percentage);
    }

    public static bool PercentageRoll(double percentage)
    {
        return RandomDouble(0.0, 100.0) <= percentage;
    }

    public static string ColorToHex(UnityEngine.Color32 color)
    {
        return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
    }

    public static UnityEngine.Color32 HexToColor(string hex)
    {
        hex = hex.Replace("#", "");

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new UnityEngine.Color32(r, g, b, 255);
    }

    public static bool ConsistsOfWhiteSpace(string s)
    {
        foreach (char c in s)
        {
            if (c != ' ') return false;
        }
        return true;

    }

    public static string RepeatString(this string input, int count)
    {
        if (!string.IsNullOrEmpty(input) && count > 0)
        {
            StringBuilder builder = new StringBuilder(input.Length * count);
            for (int i = 0; i < count; i++)
                builder.Append(input);

            return builder.ToString();
        }

        return string.Empty;
    }

    public static bool IsStringValueInEnum<T>(string value)
    {
        return Enum.GetValues(typeof(T)).Cast<T>().Any(e => e.ToString().ToUpper() == value.ToUpper());
    }

    public static T ParseEnum<T>(string value)
    {
        value = FirstLetterToUpper(value.ToLower());
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static string FirstLetterToUpper(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    /// <summary>
    /// https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
    /// </summary>
    public static int Hash(List<object> values)
    {
        int hash = 23;

        foreach(object value in values)
            hash = hash * 31 + value.GetHashCode();

        return hash;
    }

}
