using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaEnums
{
    public sealed class LuaEnumItem
    {
        public string Name { get; init; }
        public int Value { get; init; }
        public ReadOnlyCollection<string> Tags { get; init; }
    }
}
