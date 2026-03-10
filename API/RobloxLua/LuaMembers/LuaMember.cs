using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua.LuaMembers
{
    public class LuaMember
    {
        public MemberType MemberType { get; private set; }
        public string Name { get; private set; }
        public string ThreadSafety { get; private set; }
        public ReadOnlyCollection<string> Tags { get; private set; }
        public string RawData { get; private set; }

        internal virtual void FillMembers(string data)
        {
            dynamic rawData = JObject.Parse(data);

            if (Enum.TryParse<MemberType>(Convert.ToString(rawData.MemberType), out MemberType type))
            {
                MemberType = type;
            }
            else
            {
                MemberType = MemberType.Unknown;
            }
            Name = rawData.Name;
            ThreadSafety = rawData.ThreadSafety;

            List<string> tags = new();
            if (rawData.Tags != null)
            {
                foreach (dynamic tag in rawData.Tags)
                {
                    tags.Add(Convert.ToString(tag));
                }
            }
            Tags = tags.AsReadOnly();
            RawData = Convert.ToString(rawData);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LuaMember {Name} ThreadSafe: {ThreadSafety} Tags: [{string.Join(", ", Tags)}]";
        }
    }
}
