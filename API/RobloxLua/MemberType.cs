namespace RoSharp.API.RobloxLua
{
    /// <summary>
    /// Indicates the type of member.
    /// </summary>
    public enum MemberType
    {
        /// <summary>
        /// Unknown member type.
        /// </summary>
        Unknown,

        /// <inheritdoc cref="LuaMembers.LuaProperty" />
        Property,

        /// <inheritdoc cref="LuaMembers.LuaEvent" />
        Event,

        /// <inheritdoc cref="LuaMembers.LuaMethod" />
        Function,

        /// <inheritdoc cref="LuaMembers.LuaCallback" />
        Callback,
    }
}
