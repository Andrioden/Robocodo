using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ListExtensions
{
    // Ex: collection.TakeLast(5);
    // does not remove elements, only "copies" them.
    public static List<T> PopLast<T>(this List<T> source, int N)
    {
        N = Math.Min(source.Count, N);

        List<T> popped = source.TakeLast(N).ToList();
        source.RemoveRange(source.Count - N, N);
        return popped;
    }

    public static T TakeRandom<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default(T);

        int r = Utils.rnd.Next(list.Count);
        return list[r];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Utils.rnd.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
