using Newtonsoft.Json.Linq;
using RoSharp.API.RobloxLua.LuaMembers;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua
{
    public sealed class APIDumpSnapshot
    {
        public string VersionString { get; }
        public string RawDump { get; }

        public ReadOnlyCollection<LuaClass> Classes { get; }

        internal APIDumpSnapshot(string versionString, string rawDump)
        {
            VersionString = versionString;
            RawDump = rawDump;

            dynamic dump = JObject.Parse(RawDump);
            List<LuaClass> classes = new List<LuaClass>();
            foreach (dynamic rawClass in dump.Classes)
            {
                List<string> tags = new();
                if (rawClass.Tags != null)
                {
                    foreach (dynamic tag in rawClass.Tags)
                    {
                        tags.Add(Convert.ToString(tag));
                    }
                }

                LuaClass class1 = new()
                {
                    Name = rawClass.Name,
                    Superclass = rawClass.Superclass,
                    MemoryCategory = rawClass.MemoryCategory,
                    Tags = tags.AsReadOnly(),
                };
                List<LuaMember> members = new List<LuaMember>();
                foreach (dynamic rawMember in rawClass.Members)
                {
                    LuaMember member;
                    _ = Convert.ToString(rawMember.MemberType) switch
                    {
                        "Property" => member = new LuaProperty(),
                        "Function" => member = new LuaMethod(),
                        "Event" => member = new LuaEvent(),
                        "Callback" => member = new LuaCallback(),
                        _ => member = new LuaMember()
                    };
                    member.FillMembers(Convert.ToString(rawMember));
                    members.Add(member);
                }
                class1.Members = members.AsReadOnly();
                classes.Add(class1);
            }
            Classes = classes.AsReadOnly();
        }
    }
}
