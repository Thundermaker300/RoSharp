using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaEnums
{
    /// <summary>
    /// Represents a single item within a <see cref="LuaEnum"/>.
    /// </summary>
    public sealed class LuaEnumItem
    {
        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the underlying integer value of the item.
        /// </summary>
        public int Value { get; init; }

        /// <summary>
        /// Gets the tags applied to the item.
        /// </summary>
        public ReadOnlyCollection<string> Tags { get; init; }

        /// <summary>
        /// Gets the raw string data from the API Dump for this enum item.
        /// </summary>
        public string RawData { get; init; }
    }
}
