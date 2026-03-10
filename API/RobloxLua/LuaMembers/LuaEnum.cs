using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public sealed class LuaEnum
    {
        public string Name { get; init; }
        public ReadOnlyCollection<LuaEnumItem> Items { get; init; }
        public ReadOnlyCollection<string> Tags { get; init; }

        public int NumItems => Items.Count;
    }
}
