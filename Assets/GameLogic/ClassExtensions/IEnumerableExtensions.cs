using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class IEnumerableExtensions
{
    // Ex: collection.TakeLast(5);
    // does not remove elements, only "copies" them.
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
    {
        return source.Skip(Math.Max(0, source.Count() - N));
    }

    public static T TakeRandom<T>(this IEnumerable<T> source)
    {
        return source.ToList().TakeRandom();
    }

    public static IEnumerable<int> FindAllIndexes<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (T element in source)
        {
            if (predicate(element))
            {
                yield return index;
            }
            index++;
        }
    }

}