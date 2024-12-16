using RoSharp.Interfaces;
using System.Collections.Concurrent;

namespace RoSharp.API.Pooling
{
    internal static class RoPool<T>
        where T: class, IIdApi<T>
    {
        internal static ConcurrentDictionary<string, ConcurrentDictionary<ulong, T>> Pool = new();

        internal static void VerifyScope(string scope = "default")
        {
            if (!Pool.ContainsKey(scope))
                Pool.TryAdd(scope, new ConcurrentDictionary<ulong, T>());
        }

        internal static void Add(T asset, string scope = "default")
        {
            VerifyScope(scope);
            Pool[scope].TryAdd(asset.Id, asset);
        }

        internal static T Get(ulong id, Session? session = null, string scope = "default")
        {
            VerifyScope(scope);
            if (Pool[scope].TryGetValue(id, out T? result) && result != null)
            {
                return result.AttachSessionAndReturn(session);
            }
            throw new IndexOutOfRangeException($"The given id is not present in the RoPool for the {typeof(T).Name} type.");
        }

        internal static bool Contains(ulong id, string scope = "default")
        {
            VerifyScope(scope);
            return Pool[scope].ContainsKey(id);
        }
        internal static bool Contains(T item, string scope = "default")
        {
            VerifyScope(scope);
            return Pool[scope].Any(pair => pair.Value.Equals(item));
        }
    }
}
