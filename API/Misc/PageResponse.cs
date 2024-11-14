using System.Collections;
using System.Collections.ObjectModel;

namespace RoSharp.API.Misc
{
    /// <summary>
    /// Represents a response from the Roblox API that can be paged (usually via a 'cursor' parameter).
    /// </summary>
    /// <typeparam name="T">The return type from the Roblox API.</typeparam>
    public class PageResponse<T> : IEnumerable<T>
        where T: class
    {
        /// <summary>
        /// Gets a list of <typeparamref name="T"/> instances that were apart of this API response.
        /// </summary>
        public ReadOnlyCollection<T> List { get; }

        /// <summary>
        /// Gets the cursor for the next page. Can be <see langword="null"/>.
        /// </summary>
        public string? NextPageCursor { get; }

        /// <summary>
        /// Gets the cursor for the previous page. Can be <see langword="null"/>.
        /// </summary>
        public string? PreviousPageCursor { get; }

        internal PageResponse(List<T> list, string? nextPageCursor, string? previousPageCursor)
        {
            List = list.AsReadOnly();
            NextPageCursor = nextPageCursor;
            PreviousPageCursor = previousPageCursor;
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
