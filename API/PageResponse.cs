using System.Collections;
using System.Collections.ObjectModel;

namespace RoSharp.API
{
    /// <summary>
    /// Represents a list response from the Roblox API that contains a <see cref="NextPageCursor"/> and sometimes a <see cref="PreviousPageCursor"/>, allowing for repeat calls using these cursors to advance through large APIs.
    /// </summary>
    /// <typeparam name="T">The return type from the Roblox API.</typeparam>
    public sealed class PageResponse<T> : IEnumerable<T>
        where T: notnull
    {
        /// <summary>
        /// Gets an empty <see cref="PageResponse{T}"/> with an empty list and no cursors.
        /// </summary>
        public static PageResponse<T> Empty => new([], null, null);

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

        /// <summary>
        /// Indicates whether or not there is another page after this one.
        /// </summary>
        public bool IsNextPage => NextPageCursor != null;

        /// <summary>
        /// Indicates whether or not there is a page before this one.
        /// </summary>
        public bool IsPreviousPage => PreviousPageCursor != null;
        
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
