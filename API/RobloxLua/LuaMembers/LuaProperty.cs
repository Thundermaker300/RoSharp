using Newtonsoft.Json.Linq;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    /// <summary>
    /// A property represents a data point in a class. Some properties are read-only and provided by the Roblox engine, while others can be scripted to change behavior of an object.
    /// </summary>
    public class LuaProperty : LuaMember
    {
        /// <summary>
        /// Gets the internal category of the property.
        /// </summary>
        public string Category { get; private set; }

        /// <summary>
        /// Gets the security level required to access this property.
        /// </summary>
        public string ReadSecurity { get; private set; }

        /// <summary>
        /// Gets the security level required to write to this property.
        /// </summary>
        public string WriteSecurity { get; private set; }

        /// <summary>
        /// Indicates whether or not this property's value can be loaded from a saved file or string.
        /// </summary>
        public bool SerializationCanLoad { get; private set; }

        /// <summary>
        /// Indicates whether or not this property's value can be saved to a saved file or string.
        /// </summary>
        public bool SerializationCanSave { get; private set; }

        /// <summary>
        /// Gets the underlying value type of this property.
        /// </summary>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaProperty {Name} [{Category}] ThreadSafe: {ThreadSafety} Tags: [{string.Join(", ", Tags)}] || ValueType: [{ValueType}] || Security [R: {ReadSecurity} W: {WriteSecurity}] Serialization [L: {SerializationCanLoad} S: {SerializationCanSave}]";
        }
    }
}
