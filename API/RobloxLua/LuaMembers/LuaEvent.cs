using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// An event represents an execution of an action within an experience. Most events can be listened into by scripts.
    /// </summary>
    public class LuaEvent : LuaMember
    {
        /// <summary>
        /// Gets the security level required to access this event.
        /// </summary>
        public string Security { get; private set; }
        
        /// <summary>
        /// Gets a list of parameters that are included when the event is executed.
        /// </summary>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaEvent {Name} ThreadSafe: {ThreadSafety} Tags: [{string.Join(", ", Tags)}] || Security [{Security}] || Parameters: [{string.Join(", ", Parameters)}]";
        }
    }
}
