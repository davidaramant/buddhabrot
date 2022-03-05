using System;
using System.Collections.Generic;

namespace Buddhabrot.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> sequence, int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Must be greater than zero.");

        using (IEnumerator<T> enumerator = sequence.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                yield return TakeIEnumerator(enumerator, size);
            }
        }
    }

    private static IEnumerable<T> TakeIEnumerator<T>(IEnumerator<T> source, int size)
    {
        int i = 0;
        do
            yield return source.Current;
        while (++i < size && source.MoveNext());
    }
}