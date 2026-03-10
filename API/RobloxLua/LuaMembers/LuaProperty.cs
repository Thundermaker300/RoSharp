using Newtonsoft.Json.Linq;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public class LuaProperty : LuaMember
    {
        public string Category { get; private set; }
        public string ReadSecurity { get; private set; }
        public string WriteSecurity { get; private set; }

        public bool SerializationCanLoad { get; private set; }
        public bool SerializationCanSave { get; private set; }

        public LuaType ValueType { get; private set; }

        internal override void FillMembers(string data)
        {
            base.FillMembers(data);

            dynamic rawData = JObject.Parse(data);

            ValueType = new()
            {
                Category = rawData.ValueType.Category,
                Name = rawData.ValueType.Name,
            };

            if (rawData.Category != null)
            {
                Category = rawData.Category;
            }


            ReadSecurity = rawData.Security.Read;
            WriteSecurity = rawData.Security.Write;

            SerializationCanLoad = rawData.Serialization.CanLoad;
            SerializationCanSave = rawData.Serialization.CanSave;

        }
    }
}
