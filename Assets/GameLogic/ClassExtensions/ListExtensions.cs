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

}
