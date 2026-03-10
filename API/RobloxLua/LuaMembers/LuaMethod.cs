using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// A method represents a function as part of a class that can be executed anytime by a game script.
    /// </summary>
    public class LuaMethod : LuaMember
    {
        /// <summary>
        /// Gets a list of parameters (required or optional) in the method.
        /// </summary>
        public ReadOnlyCollection<LuaParameter> Parameters { get; private set; }

        /// <summary>
        /// Gets a list of return types from the method's execution.
        /// </summary>
        public ReadOnlyCollection<LuaType> ReturnType { get; private set; }

        /// <summary>
        /// Gets the security level required to access this method.
        /// </summary>
        public string Security { get; private set; }

        /// <summary>
        /// Gets access capabilities for this method.
        /// </summary>
        public ReadOnlyCollection<string> Capabilities { get; private set; }

        internal override void FillMembers(string data)
        {
            base.FillMembers(data);

            dynamic rawData = JObject.Parse(data);

            Security = rawData.Security;

            List<LuaType> returnList = new();
            if (rawData.ReturnType is JArray array)
            {
                foreach (dynamic item in array.Children())
                {
                    returnList.Add(new LuaType()
                    {
                        Category = item.Category,
                        Name = item.Name,
                    });
                }
            }
            else
            {
                returnList.Add(new LuaType()
                {
                    Category = rawData.ReturnType.Category,
                    Name = rawData.ReturnType.Name,
                });
            }
            ReturnType = returnList.AsReadOnly();

            List<LuaParameter> parameters = new();
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

            List<string> capabilities = new();
            if (rawData.Capabilities != null)
            {
                foreach (dynamic obj in rawData.Capabilities)
                {
                    capabilities.Add(Convert.ToString(obj));
                }
            }
            Capabilities = capabilities.AsReadOnly();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaMethod {Name} ThreadSafe: {ThreadSafety} Tags: [{string.Join(", ", Tags)}] || Security [{Security}] || Parameters: [{string.Join(", ", Parameters)}] || ReturnType: [{string.Join(", ", ReturnType)}] || Capabilities: [{string.Join(", ", Capabilities)}]";
        }
    }
}
