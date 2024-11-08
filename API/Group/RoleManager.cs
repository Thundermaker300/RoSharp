﻿using Newtonsoft.Json.Linq;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class RoleManager : IRefreshable
    {
        private Group group;

        public DateTime RefreshedAt { get; set; }

        private ReadOnlyCollection<Role>? roles;
        public ReadOnlyCollection<Role> Roles
        {
            get
            {
                if (roles == null)
                {
                    List<Role> list = [];
                    string rawData = group.GetString($"/v1/groups/{group.Id}/roles", verifySession: false);
                    dynamic data = JObject.Parse(rawData);
                    foreach (dynamic rank in data.roles)
                    {
                        Role role = new Role(this)
                        {
                            Id = rank.id,
                            Name = rank.name,
                            Rank = rank.rank,
                            MemberCount = rank.memberCount,
                        };
                        list.Add(role);
                    }

                    roles = list.AsReadOnly();
                    RefreshedAt = DateTime.Now;
                }
                return roles;
            }
        }

        public void Refresh()
        {
            roles = null;
        }

        internal RoleManager(Group group) { this.group = group; }

        public Role? GetRole(int rankId)
            => Roles.FirstOrDefault(role => role.Rank == rankId);

        public Role? GetRole(string roleName)
            => Roles.FirstOrDefault(role => role.Name == roleName);

        internal async Task RequestDeleteRole(ulong roleId)
        {
            HttpResponseMessage response = await group.DeleteAsync($"/v1/groups/{group.Id}/rolesets/{roleId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Roleset delete failed (HTTP {response.StatusCode}). Do you have permission to delete this group's rolesets?");
            }
        }

        internal async Task RequestUpdateRole(Role roleId, string newName)
        {
            object body = new { name = newName, rank = roleId.Rank };
            JsonContent content = JsonContent.Create(body);
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Roleset modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's rolesets?");
            }
        }

        internal async Task RequestUpdateRole(Role roleId, int newRank)
        {
            object body = new { name = roleId.Name, rank = newRank };
            JsonContent content = JsonContent.Create(body);
            HttpResponseMessage response = await group.PatchAsync($"/v1/groups/{group.Id}/rolesets/{roleId.Id}", body);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Roleset modification failed (HTTP {response.StatusCode}). Do you have permission to modify this group's rolesets?");
            }
        }

        public override string ToString()
        {
            return $"RoleManager [#{Roles.Count}]";
        }
    }

    public class Role
    {
        internal RoleManager roleManager;

        public ulong Id { get; init; }
        public string Name { get; init; }
        public int Rank { get; init; }
        public ulong MemberCount { get; init; }

        internal Role(RoleManager manager) { this.roleManager = manager; }

        [UsesSession]
        public async Task UpdateAsync(string newName)
        {
            await roleManager.RequestUpdateRole(this, newName);
        }

        [UsesSession]
        public async Task UpdateAsync(int newRank)
        {
            await roleManager.RequestUpdateRole(this, newRank);
        }

        [UsesSession]
        public async Task DeleteAsync()
        {
            // TODO: This doesnt' work (forbidden)
            throw new Exception("This method has been disabled.");
            await roleManager.RequestDeleteRole(Id);
        }
    }
}
