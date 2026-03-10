using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public class LuaEvent : LuaMember
    {
        public string Security { get; private set; }
        public ReadOnlyCollection<LuaParameter> Parameters { get; private set; }

        internal override void FillMembers(string data)
        {
            base.FillMembers(data);

            dynamic rawData = JObject.Parse(data);

            Security = rawData.Security;

            List<LuaParameter> parameters = new List<LuaParameter>();
            foreach (dynamic parameter in rawData.Parameters)
            {
                var item = new LuaParameter()
                {
                    Name = parameter.Name,
                    Type = new()
                    {
                        Category = parameter.Type.Category,
                        Name = parameter.Type.Name,
                    }
                };
                parameters.Add(item);
            }
            Parameters = parameters.AsReadOnly();
        }
    }
}
