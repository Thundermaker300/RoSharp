using RoSharp.Interfaces;
using RoSharp.Utility;

namespace RoSharp.API
{
    /// <summary>
    /// Represents an Id corresponding to the given <typeparamref name="T"/>.
    /// <para>
    /// This class is created instead of <typeparamref name="T"/> directly in large API requests to avoid Roblox ratelimits where the program does not need all of the associated item's data.
    /// To return the associated item with all of its data, see <see cref="GetInstanceAsync(Session?)"/>.
    /// </para>
    /// <para>
    /// If a <see cref="Session"/> is provided in the API that returns this method (either directly in the method or indirectly in the class), that session will also be associated with the instance returned by <see cref="GetInstanceAsync(Session?)"/> automatically.
    /// A session can be provided to this method directly to override the session for the returned instance.
    /// If all else fails, the global session from <see cref="GlobalSession"/> will be used if it is assigned.
    /// </para>
    /// </summary>
    public sealed class Id<T>
        where T: IIdApi<T>
    {
        private T? stored;
        private Session? storedSession;

        /// <summary>
        /// Creates a new <see cref="Id{T}"/> with the given Id.
        /// </summary>
        /// <param name="id">The user Id.</param>
        /// <param name="session">The session. Optional.</param>
        public Id(ulong id, Session? session = null)
        {
            UniqueId = id;

            storedSession = session;
        }

        /// <summary>
        /// Gets the Id of the <typeparamref name="T"/>.
        /// </summary>
        public ulong UniqueId { get; }

        /// <summary>
        /// Gets the <see cref="System.Type"/> that is being encapsulated by this object. Equivalent to <c>typeof(<typeparamref name="T"/>).</c>
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Returns the <typeparamref name="T"/> associated with this Id.
        /// <para>
        /// This API member calls the "FromId" API member of the <typeparamref name="T"/>.
        /// As such, at least one API request is made (sometimes more depending on the type -- experiences make at least 3 for example).
        /// Keep this in mind if doing requests en-masse.
        /// Any subsequent calls to this method (or the "FromId" method of the original <typeparamref name="T"/>) will return a cached value. If the returned <typeparamref name="T"/> is a <see cref="IRefreshable"/>, take advantage of <see cref="IRefreshable.RefreshAsync"/> if you are expecting data to change.
        /// </para>
        /// </summary>
        /// <returns>A task that contains the <typeparamref name="T"/> instance upon completion.</returns>
        public async Task<T> GetInstanceAsync(Session? session = null)
        {
            Session? sessionToUse = session ?? storedSession ?? GlobalSession.Assigned;
            stored ??= await T.FromId(UniqueId, sessionToUse);
            return stored;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return UniqueId.ToString();
        }

        /// <summary>
        /// Converts the <see cref="Id{T}"/> to its Id. Equivalent to accessing <see cref="Id{T}.UniqueId"/>.
        /// </summary>
        /// <param name="id">The <see cref="Id{T}"/> to convert.</param>
        public static implicit operator ulong(Id<T> id) => id.UniqueId;
    }
}
