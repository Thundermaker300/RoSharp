namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// Represents a parameter in a <see cref="LuaMethod"/>, <see cref="LuaEvent"/>, or <see cref="LuaCallback"/>.
    /// </summary>
    public struct LuaParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the expected type of the parameter.
        /// </summary>
        public LuaType Type { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaParameter {Name} Type: [{Type}]";
        }
    }
}
