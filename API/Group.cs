using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoSharp.API
{
    public class Group : APIMain
    {
        public override string BaseUrl => "https://groups.roblox.com";

        public ulong Id { get; }
        public string Name { get; }
        public string Description { get; }
        public User? Owner { get; }
        public bool HasOwner => Owner != null;
        public ulong Members { get; }
        public bool IsPublic { get; }
        public bool Verified { get; }
        public bool GroupFundsVisible { get; }
        public bool GroupExperiencesVisible { get; }

        private RoleManager roleManager;
        public RoleManager RoleManager => roleManager ?? new RoleManager(this);

        public Group(ulong groupId)
        {
            HttpResponseMessage response = Get($"/v1/groups/{groupId}", verifySession: false);
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(raw);

                Id = data.id;
                Name = data.name;
                Description = data.description;

                if (data.owner != null)
                {
                    Owner = new(Convert.ToUInt64(data.owner.userId));
                }

                Members = data.memberCount;
                IsPublic = data.publicEntryAllowed;
                Verified = data.hasVerifiedBadge;
            }
            else
            {
                throw new InvalidOperationException("Invalid group ID");
            }
        }

        public GroupShoutInfo? shout;
        public GroupShoutInfo? Shout
        {
            get
            {
                if (shout == null)
                {
                    string rawData = GetString($"/v1/groups/{Id}");
                    dynamic data = JObject.Parse(rawData);
                    if (data.shout != null)
                    {
                        shout = new GroupShoutInfo
                        {
                            Text = data.shout.body,
                            Poster = new User(Convert.ToUInt64(data.shout.poster.userId)),
                            PostedAt = data.shout.updated,
                        };
                    }
                }
                return shout;
            }
        }

        [UsesSession]
        public async Task ModifyDescriptionAsync(string text)
        {
            object body = new { description = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{Id}/description", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group description modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's description?");
            }
        }

        public async Task ShoutAsync(string text)
        {
            object body = new { message = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{Id}/status", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group shout failed (HTTP {response.StatusCode}). Do you have permission to modify this group's status?");
            }
        }

        public async Task<ReadOnlyCollection<GroupPost>> GetGroupPostsAsync()
        {
            var list = new List<GroupPost>();
            HttpResponseMessage response = Get($"v2/groups/{Id}/wall/posts?limit=100&sortOrder=Desc");
            if (response.IsSuccessStatusCode)
            {
                dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                foreach (dynamic post in data.data)
                {
                    list.Add(new GroupPost()
                    {
                        PostId = post.id,
                        Poster = new User(Convert.ToUInt64(post.poster.user.userId)),
                        PostedAt = post.updated,
                        Text = post.body,
                        RankInGroup = post.poster.role.name,
                    });
                }
            }

            return list.AsReadOnly();
    }

        public Group AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }

    public class GroupShoutInfo
    {
        public string Text { get; init; }
        public User Poster { get; init; }
        public DateTime PostedAt { get; init; }
    }

    public class GroupPost
    {
        public ulong PostId { get; init; }
        public string Text { get; init; }
        public User Poster { get; init; }
        public DateTime PostedAt { get; init; }
        public string RankInGroup { get; init; }
    }
}
