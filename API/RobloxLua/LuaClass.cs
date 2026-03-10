using RoSharp.API.RobloxLua.LuaMembers;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua
{
    /// <summary>
    /// Represents a class, which is an object that provides information in the form of <see cref="LuaProperty">properties</see>, provides <see cref="LuaProperty">properties</see> and/or <see cref="LuaMethod">methods</see> to modify the world, or behaves a specific way within the world that can be controlled by scripts.
    /// </summary>
    public class LuaClass
    {
        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the memory category of the class.
        /// </summary>
        public string MemoryCategory { get; init; }

        /// <summary>
        /// Gets the class that this class inherits directly from.
        /// </summary>
        public string Superclass { get; init; }


        /// <summary>
        /// Gets the raw string data from the API Dump for this class.
        /// </summary>
        public string RawData { get; init; }

        /// <summary>
        /// Gets the tags applied to the class.
        /// </summary>
        public ReadOnlyCollection<string> Tags { get; init; }

        /// <summary>
        /// Gets this classes' members.
        /// </summary>
        public ReadOnlyCollection<LuaMember> Members { get; internal set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaClass {Name} Tags: [{string.Join(", ", Tags)}] || Superclass [{Superclass}] || MemoryCategory {MemoryCategory} || Members [{string.Join(", ", Members)}]";
        }
    }
}
