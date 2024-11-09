using RoSharp.API.Assets;
using System.Collections.Concurrent;

namespace RoSharp.API.Pooling
{
    internal static class RoPool<T>
        where T: class, IPoolable
    {
        internal static ConcurrentDictionary<ulong, T> Pool = new();

        internal static void Add(T asset) => Pool.TryAdd(asset.Id, asset);
        internal static T? Get(ulong id, Session? session = null)
            => (Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session)) as T;

        internal static bool Contains(ulong id) => Pool.ContainsKey(id);
        internal static bool Contains(T item) => Pool.Any(pair => pair.Value.Equals(item));
    }
}
