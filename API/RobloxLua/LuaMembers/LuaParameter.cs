namespace RoSharp.API.RobloxLua.LuaMembers
{
    public struct LuaParameter
    {
        public string Name { get; init; }
        public LuaType Type { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaParameter {Name} Type: [{Type}]";
        }
    }
}
