namespace Buddhabrot.Core.ExtensionMethods.Collections;

public static class CollectionExtensions
{
    public static TValue RemoveOr<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue fallback)
        where TKey : notnull
    {
        if (dict.TryGetValue(key, out var value))
        {
            dict.Remove(key);
            return value;
        }

        return fallback;
    }
}
