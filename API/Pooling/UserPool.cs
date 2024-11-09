using RoSharp.API.Assets;

namespace RoSharp.API.Pooling
{
    internal static class UserPool
    {
        internal static Dictionary<ulong, User> Pool = new();

        internal static void Add(User user) => Pool.Add(user.Id, user);
        internal static User? Get(ulong id, Session? session = null) => Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session);

        internal static bool Contains(ulong userId) => Pool.ContainsKey(userId);
        internal static bool Contains(User user) => Pool.ContainsValue(user);
    }
}
