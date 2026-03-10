using RoSharp.API.RobloxLua.LuaMembers;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua
{
    public class LuaClass
    {
        public string Name { get; init; }
        public string MemoryCategory { get; init; }
        public string Superclass { get; init; }
        public ReadOnlyCollection<string> Tags { get; init; }
        public ReadOnlyCollection<LuaMember> Members { get; internal set; }
    }
}
