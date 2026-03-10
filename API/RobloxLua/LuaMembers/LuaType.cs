namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// Represents the underlying type of a property, parameter, or return type.
    /// </summary>
    public struct LuaType
    {
        /// <summary>
        /// The category of the type.
        /// </summary>
        public string? Category { get; init; }

        /// <summary>
        /// The name of the type.
        /// </summary>
        public string? Name { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaType {Name} [{Category}]";
        }
    }
}
