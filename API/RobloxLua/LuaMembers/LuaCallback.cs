using Newtonsoft.Json.Linq;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public class LuaCallback : LuaMethod
    {
        public string Security { get; private set; }

        internal override void FillMembers(string data)
        {
            base.FillMembers(data);

            dynamic rawData = JObject.Parse(data);

            Security = rawData.Security;
        }
    }
}
