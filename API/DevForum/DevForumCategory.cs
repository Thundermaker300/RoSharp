using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.DevForum
{
    public class DevForumCategory
    {
        public string Title { get; init; }
        public string Description { get; init; }
        public string Color { get; init; }
        public ushort Position { get; init; }
        public ushort Id { get; init; }
        public int DisplayTopics { get; init; }
        public ReadOnlyCollection<ushort> SubcategoryIds { get; init; }
        public ReadOnlyCollection<ulong> TopicIds { get; init; }

        public async Task<ReadOnlyCollection<DevForumTopic>> GetTopicsAsync(bool excludeSystemReplies = true)
        {
            List<DevForumTopic> list = new();

            foreach (var topic in TopicIds)
            {
                list.Add(await DevForumAPI.GetTopicAsync(topic, excludeSystemReplies));
            }
            return list.AsReadOnly();
        }
    }
}
