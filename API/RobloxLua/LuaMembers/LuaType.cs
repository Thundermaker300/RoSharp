namespace RoSharp.API.RobloxLua.LuaMembers
{
    public struct LuaType
    {
        public string? Category { get; init; }
        public string? Name { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaType {Name} [{Category}]";
        }
    }
}
