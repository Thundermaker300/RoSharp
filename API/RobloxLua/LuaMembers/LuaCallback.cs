using Newtonsoft.Json.Linq;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public class LuaCallback : LuaMethod
    {
        internal override void FillMembers(string data)
        {
            base.FillMembers(data);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaCallback {Name} ThreadSafe: {ThreadSafety} Tags: [{string.Join(", ", Tags)}] || Security [{Security}] || Parameters: [{string.Join(", ", ReturnType)}] || ReturnType: [{string.Join(", ", ReturnType)}] || Capabilities: [{string.Join(", ", Capabilities)}]";
        }
    }
}
