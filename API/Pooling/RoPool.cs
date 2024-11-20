using RoSharp.API.Assets;
using RoSharp.Interfaces;
using System.Collections.Concurrent;

namespace RoSharp.API.Pooling
{
    internal static class RoPool<T>
        where T: class, IIdApi<T>
    {
        internal static ConcurrentDictionary<ulong, T> Pool = new();

        internal static void Add(T asset) => Pool.TryAdd(asset.Id, asset);
        internal static T? Get(ulong id, Session? session = null)
        {
            if (Pool.TryGetValue(id, out T result) && result != null)
            {
                return result.AttachSessionAndReturn(session);
            }
            return null;
        }

        internal static bool Contains(ulong id) => Pool.ContainsKey(id);
        internal static bool Contains(T item) => Pool.Any(pair => pair.Value.Equals(item));
    }
}
