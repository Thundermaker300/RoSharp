using RoSharp.API;
using RoSharp.Exceptions;
using System.Collections.ObjectModel;
using RoSharp.API.Assets.Experiences;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a category for experiences within the Charts API.
    /// </summary>
    public sealed class ChartCategory
    {
        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// Gets the internal Id of the category.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the description of the category.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Gets a list of experience Ids within this category.
        /// </summary>
        public ReadOnlyCollection<GenericId<Experience>> ExperienceIds { get; init; }

        /// <summary>
        /// Converts <see cref="ExperienceIds"/> to a read-only <see cref="Experience"/> collection.
        /// </summary>
        /// <param name="limit">The maximum amount of Ids to convert. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before converting.</param>
        /// <returns>A task containing the list of experiences upon completion.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only convert a certain amount of experiences at a time.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task<ReadOnlyCollection<Experience>> ToExperienceListAsync(int limit = -1, int startAt = 0)
        {
            List<Experience> experiences = [];
            foreach (GenericId<Experience> id in ExperienceIds.Skip(startAt))
            {
                experiences.Add(await id.GetInstanceAsync());

                if (limit > 0 && experiences.Count >= limit)
                    break;
            }
            return experiences.AsReadOnly();
        }

        /// <summary>
        /// Calls a provided action for each experience in the <see cref="ExperienceIds"/> list, converting each to a <see cref="Experience"/> before doing so.
        /// </summary>
        /// <param name="func">The action to run. Can be asynchronous.</param>
        /// <param name="limit">The maximum amount of Ids to call the <paramref name="func"/> with. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before calling the <paramref name="func"/>.</param>
        /// <returns>A task that completes when the process is done.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only peform the action with a certain amount of experiences at a time.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task ForEachExperienceAsync(Func<Experience, Task> func, int limit = -1, int startAt = 0)
        {
            int count = 0;
            foreach (GenericId<Experience> id in ExperienceIds.Skip(startAt))
            {
                await func(await id.GetInstanceAsync());
                count++;

                if (limit > 0 && count >= limit)
                    break;
            }
        }

        /// <summary>
        /// Calls a provided action for each experience in the <see cref="ExperienceIds"/> list, converting each to a <see cref="Experience"/> before doing so.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="limit">The maximum amount of Ids to call the <paramref name="action"/> with. Defaults to <c>-1</c> (unlimited).</param>
        /// <param name="startAt">The amount of Ids to skip before calling the <paramref name="action"/>.</param>
        /// <returns>A task that completes when the process is done.</returns>
        /// <remarks>This API is very time-consuming as each new experience is an API call, and Too Many Requests is a common error. As such, a limit should be used in conjunction with the <paramref name="startAt"/> parameter to only peform the action with a certain amount of experiences at a time.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public async Task ForEachExperienceAsync(Action<Experience> action, int limit = -1, int startAt = 0)
        {
            int count = 0;
            foreach (GenericId<Experience> id in ExperienceIds.Skip(startAt))
            {
                action(await id.GetInstanceAsync());
                count++;

                if (limit > 0 && count >= limit)
                    break;
            }
        }
    }
}
