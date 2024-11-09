using RoSharp.API.Assets;

namespace RoSharp.API.Pooling
{
    internal static class AssetPool
    {
        internal static Dictionary<ulong, Asset> Pool = new();

        internal static void Add(Asset asset) => Pool.Add(asset.Id, asset);
        internal static Asset? Get(ulong id, Session? session = null) => Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session);

        internal static bool Contains(ulong assetId) => Pool.ContainsKey(assetId);
        internal static bool Contains(Asset asset) => Pool.ContainsValue(asset);
    }
}
