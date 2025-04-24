namespace RoSharp.API.Communities.Forum
{
    public sealed class ForumComment
    {
        internal static ForumComment Construct(CommunityForum manager, ForumCategory category, dynamic data)
        {
            return new ForumComment
            {
                Id = data.id,
                ParentId = data.parentId,
                Category = category,
                Text = data.content.plainText,
                Poster = new(Convert.ToUInt64(data.createdBy), manager.community.session),
                Created = data.createdAt,
                Updated = data.updatedAt,
                ReplyCount = data.threadCommentCount ?? 0,
                
                manager = manager
            };
        }

        internal CommunityForum manager;

        public string Id { get; init; }
        public string ParentId { get; init; }
        public ForumCategory Category { get; init; }
        public string Text { get; init; }
        public Id<User> Poster { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public int ReplyCount { get; init; }

        public bool IsEdited => Created.Ticks != Updated.Ticks;

        // Todo: Add reactions

        // Todo: Implement when I figure out how the cursor system for forum comment replies works
        /*
        public async Task<PageResponse<ForumComment>> GetRepliesAsync(string? cursor = null)
        {
            return null;
        }*/

    }
}
