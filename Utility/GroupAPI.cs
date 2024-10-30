using Newtonsoft.Json.Linq;
using RoSharp.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class GroupAPI : APIMain
    {
        public override string BaseUrl => "https://groups.roblox.com";
        public GroupAPI(Session session) : base(session) { }
        public GroupAPI() : base() { }

        public GroupData GetGroupInfo(ulong groupId)
        {
            string rawData = GetString($"/v1/groups/{groupId}", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            if (data.errors != null)
            {
                throw new InvalidOperationException("Group does not exist or is locked. Please check the provided group ID.");
            }
            return new GroupData()
            {
                Id = data.id,
                Name = data.name,
                Description = data.description,
                Owner = new User(data.owner.userId),
                Members = data.memberCount,
                Shout = data.shout != null ?
                new GroupShoutInfo()
                {
                    Text = data.shout.body,
                    Poster = new User(data.shout.poster.userId),
                    PostedAt = data.shout.created
                }
                : null,
                IsPublic = data.publicEntryAllowed,
                Verified = data.hasVerifiedBadge,
            };
        }

        public async Task ModifyGroupDescriptionAsync(int groupId, string text)
        {
            object body = new { description = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{groupId}/description", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's description?");
            }
        }

        public async Task ShoutAsync(int groupId, string text)
        {
            object body = new { message = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{groupId}/status", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's status?");
            }
        }

        // TODO: This doesn't work lol (forbidden?)
        public async Task PostOnWallAsync(int groupId, string text)
        {
            object body = new { body = text };
            HttpResponseMessage response = await PostAsync($"/v1/groups/{groupId}/wall/posts", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's status?");
            }
        }
    }
}
