using RoSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    public class GroupData : APIMain
    {
        public ulong Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public User Owner { get; init; }
        public ulong Members { get; init; }
        public GroupShoutInfo? Shout { get; init; }
        public bool IsPublic { get; init; }
        public bool Verified { get; init; }
        public bool GroupFundsVisible { get; init; }
        public bool GroupExperiencesVisible { get; init; }
    }

    public class GroupShoutInfo()
    {
        public string Text { get; init; }
        public User Poster { get; init; }
        public DateTime PostedAt { get; init; }
    }
}
