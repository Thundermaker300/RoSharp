using Newtonsoft.Json.Linq;
using RoSharp.API.RobloxLua.LuaEnums;
using RoSharp.API.RobloxLua.LuaMembers;
using System.Collections.ObjectModel;

namespace RoSharp.API.RobloxLua
{
    public sealed class APIDumpSnapshot
    {
        public string VersionString { get; }
        public string RawDump { get; }

        public ReadOnlyCollection<LuaClass> Classes { get; }
        public ReadOnlyCollection<LuaEnum> Enums { get; }

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
                    RawData = Convert.ToString(rawClass),
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

            List<LuaEnum> enums = new();
            foreach (dynamic rawEnum in dump.Enums)
            {
                List<LuaEnumItem> items = [];
                List<string> tags = [];

                if (rawEnum.Items != null)
                {
                    foreach (dynamic item in rawEnum.Items)
                    {
                        List<string> itemTags = [];
                        if (item.Tags != null)
                        {
                            foreach (dynamic itemTag in item.Tags)
                            {
                                itemTags.Add(Convert.ToString(itemTag));
                            }
                        }
                        items.Add(new()
                        {
                            Name = item.Name,
                            Value = item.Value,
                            Tags = itemTags.AsReadOnly(),
                            RawData = Convert.ToString(item),
                        });
                    }
                }

                if (rawEnum.Tags != null)
                {
                    foreach (dynamic tag in rawEnum.Tags)
                        tags.Add(Convert.ToString(tag));
                }

                LuaEnum enum1 = new()
                {
                    Name = rawEnum.Name,
                    Items = items.AsReadOnly(),
                    Tags = tags.AsReadOnly(),
                    RawData = Convert.ToString(rawEnum)
                };
                enums.Add(enum1);
            }
            Enums = enums.AsReadOnly();
        }
    }
}
