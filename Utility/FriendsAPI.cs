using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class FriendsAPI : APIMain
    {
        public override string BaseUrl => "https://friends.roblox.com";

        public FriendsAPI(Session session) : base(session) { }
        public FriendsAPI() : base() { }

        public IEnumerable<User> GetFriends(ulong userId)
        {
            string rawData = GetString($"/v1/users/{userId}/friends");
            dynamic data = JObject.Parse(rawData);
            foreach (dynamic friendData in data.data)
            {
                yield return new User(friendData.id).AttachSessionAndReturn(session);
            }
        }

        public async Task SendFriendRequestAsync(ulong targetId)
        {
            await PostAsync($"/v1/contacts/{targetId}/request-friendship", new { });
        }

        public async Task UnfriendAsync(ulong targetId)
        {
            await PostAsync($"/v1/users/{targetId}/unfriend", new { });
        }
    }
}
