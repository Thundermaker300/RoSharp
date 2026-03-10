using Newtonsoft.Json.Linq;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// A callback is very similar to a <see cref="LuaMethod"/>. However, instead of calling it by a script, developers are expected to assign a custom function that will be called by the engine. Some callbacks have parameters, and some are expected to return a value which will change the Roblox engine's response.
    /// </summary>
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
