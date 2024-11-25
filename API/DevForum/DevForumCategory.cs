using System.Collections.ObjectModel;

namespace RoSharp.API.DevForum
{
    /// <summary>
    /// Represents a category on the Roblox Developer Forum.
    /// </summary>
    public class DevForumCategory
    {
        /// <summary>
        /// The title of the category.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// The description of the category.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// The color of the category, hex code.
        /// </summary>
        public string Color { get; init; }

        /// <summary>
        /// Gets whether or not this category is a subcategory.
        /// </summary>
        public bool IsSubcategory { get; internal set; }

        /// <summary>
        /// The position of the category relative to other categories.
        /// </summary>
        public ushort Position { get; init; }

        /// <summary>
        /// The unique Id of the category.
        /// </summary>
        public ushort Id { get; init; }

        /// <summary>
        /// The amount of topics to display at once.
        /// </summary>
        public int DisplayTopics { get; init; }

        /// <summary>
        /// A <see cref="ReadOnlyCollection{T}"/> of IDs of categories within this category. Will be <see langword="null"/> for subcategories.
        /// </summary>
        public ReadOnlyCollection<ushort>? SubcategoryIds { get; internal set; }

        /// <summary>
        /// Topic IDs of topics that are currently being shown. Will be <see langword="null"/> for subcategories.
        /// </summary>
        public ReadOnlyCollection<ulong>? TopicIds { get; internal set; }

        /// <summary>
        /// Gets the topics that are currently being shown in this category.
        /// </summary>
        /// <param name="excludeSystemReplies">Exclude system replies within topics.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="DevForumTopic"/> upon completion.</returns>
        public async Task<ReadOnlyCollection<DevForumTopic>> GetTopicsAsync(bool excludeSystemReplies = true)
        {
            List<DevForumTopic> list = new();

            foreach (var topic in TopicIds)
            {
                list.Add(await DevForumAPI.GetTopicAsync(topic, excludeSystemReplies));
            }
            return list.AsReadOnly();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Title} [{Id}] <#{Color}> -- {Description}";
        }
    }
}
