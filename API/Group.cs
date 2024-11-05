﻿using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Interfaces;
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
    public class Group : APIMain, IRefreshable
    {
        public override string BaseUrl => "https://groups.roblox.com";

        public ulong Id { get; }

        private string name;
        public string Name { get; }

        private string description;
        public string Description { get; }

        private User? owner;
        public User? Owner { get; }

        public bool HasOwner => Owner != null;

        private bool isPublic;
        public bool IsPublic { get; }

        private bool verified;
        public bool Verified { get; }


        private RoleManager roleManager;
        public RoleManager RoleManager => roleManager ?? new RoleManager(this);

        private MemberManager memberManager;
        public MemberManager MemberManager => memberManager ?? new MemberManager(this);

        public DateTime RefreshedAt { get; set; }

        internal ulong members;

        public Group(ulong groupId, Session? session = null)
        {
            Id = groupId;

            Refresh();

            if (session != null)
                AttachSession(session);
        }

        public void Refresh()
        {
            HttpResponseMessage response = Get($"/v1/groups/{Id}", verifySession: false);
            if (response.IsSuccessStatusCode)
            {
                string raw = response.Content.ReadAsStringAsync().Result;
                dynamic data = JObject.Parse(raw);

                name = data.name;
                description = data.description;

                if (data.owner != null)
                {
                    owner = new(Convert.ToUInt64(data.owner.userId));
                }

                isPublic = data.publicEntryAllowed;
                verified = data.hasVerifiedBadge;

                members = data.memberCount;
            }
            else
            {
                throw new InvalidOperationException("Invalid group ID");
            }

            // Reset properties
            shout = null;
            socialChannels = null;

            RefreshedAt = DateTime.Now;
        }

        private GroupShoutInfo? shout;
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

        private ReadOnlyDictionary<string, string>? socialChannels;
        public ReadOnlyDictionary<string, string> SocialChannels
        {
            get
            {
                if (socialChannels == null)
                {
                    Dictionary<string, string> dict = new();
                    string rawData = GetString($"/v1/groups/{Id}/social-links");
                    dynamic data = JObject.Parse(rawData);
                    foreach (dynamic media in data.data)
                    {
                        dict.Add(Convert.ToString(media.type), Convert.ToString(media.url));
                    }
                    socialChannels = dict.AsReadOnly();
                }

                return socialChannels;
            }
        }

        public async Task<string> GetIconAsync(ThumbnailSize size = ThumbnailSize.S420x420)
        {
            string url = $"/v1/groups/icons?groupIds={Id}&size={size.ToString().Substring(1)}&format=Png&isCircular=false";
            string rawData = await GetStringAsync(url, "https://thumbnails.roblox.com", verifySession: false);
            dynamic data = JObject.Parse(rawData);
            if (data.data.Count == 0)
                throw new InvalidOperationException("Invalid group to get icon for.");
            return data.data[0].imageUrl;
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

        [UsesSession]
        public async Task ShoutAsync(string text)
        {
            object body = new { message = text };
            HttpResponseMessage response = await PatchAsync($"/v1/groups/{Id}/status", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Group shout failed (HTTP {response.StatusCode}). Do you have permission to modify this group's status?");
            }
        }

        [UsesSession]
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

        public override string ToString()
        {
            return $"{Name} [{Id}] {(Verified ? "[V]" : string.Empty)}";
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
