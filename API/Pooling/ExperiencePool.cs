using RoSharp.API.Assets;

namespace RoSharp.API.Pooling
{
    internal static class ExperiencePool
    {
        internal static Dictionary<ulong, Experience> Pool = new();

        internal static void Add(Experience exp) => Pool.Add(exp.UniverseId, exp);
        internal static Experience? Get(ulong id, Session? session = null) => Pool.GetValueOrDefault(id)?.AttachSessionAndReturn(session);

        internal static bool Contains(ulong experienceId) => Pool.ContainsKey(experienceId);
        internal static bool Contains(Experience exp) => Pool.ContainsValue(exp);
    }
}
