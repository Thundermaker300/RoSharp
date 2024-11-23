using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.DevForum
{
    public struct DevForumPost
    {
        public ulong PostId { get; init; }
        public string Content { get; init; }
        public Id<User>? Poster { get; init; }
        public string PosterName { get; init; }
        public DateTime Created { get; init; }
        public DateTime LastUpdated { get; init; }
        public int Likes { get; init; }
        public int PostNumber { get; init; }
        public int ReplyCount { get; init; }
        public int Reads { get; init; }
        public int EditVersion { get; init; }
        public bool Hidden { get; init; }
        public string? UserTitle { get; init; }
        public bool AcceptedAnswer { get; init; }
    }
}
