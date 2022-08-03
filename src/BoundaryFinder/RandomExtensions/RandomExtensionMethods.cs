using System;
using System.Collections.Generic;
using System.Linq;

namespace BoundaryFinder.RandomExtensions;

public static class RandomExtensionMethods
{
    public static IEnumerable<int> GetNumbers(this Random rand, int max)
    {
        while (true)
        {
            yield return rand.Next(max);
        }
    }

    public static IEnumerable<T> GetRandomSequence<T>(this IReadOnlyList<T> list, Random random) => 
        random.GetNumbers(list.Count).Select(index => list[index]);
}