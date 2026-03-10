using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaEnums
{
    /// <summary>
    /// Represents an Enumeration, which is a collection of fixed string items with underlying integer values.
    /// </summary>
    public sealed class LuaEnum
    {
        /// <summary>
        /// Gets the name of the enum.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the items contained within the enum.
        /// </summary>
        public ReadOnlyCollection<LuaEnumItem> Items { get; init; }

        /// <summary>
        /// Gets the tags that are applied to the enum.
        /// </summary>
        public ReadOnlyCollection<string> Tags { get; init; }

        /// <summary>
        /// Gets the raw string data from the API Dump for this enum.
        /// </summary>
        public string RawData { get; init; }

        /// <summary>
        /// Gets the total amount of items contained within the enum.
        /// </summary>
        public int NumItems => Items.Count;
    }
}
