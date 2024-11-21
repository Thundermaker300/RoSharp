﻿using RoSharp.Interfaces;
using RoSharp.Utility;

namespace RoSharp.API
{
    /// <summary>
    /// Represents an Id corresponding to the given <typeparamref name="T"/>. This class is created instead of <typeparamref name="T"/> directly in large API requests to avoid Roblox ratelimits.
    /// </summary>
    public sealed class GenericId<T>
        where T: IIdApi<T>
    {
        private T? stored;
        private Session? storedSession;

        /// <summary>
        /// Creates a new <see cref="GenericId{T}"/> with the given Id.
        /// </summary>
        /// <param name="id">The user Id.</param>
        /// <param name="session">The session. Optional.</param>
        public GenericId(ulong id, Session? session = null)
        {
            Id = id;

            storedSession = session;
        }

        /// <summary>
        /// Gets the Id of the <typeparamref name="T"/>.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Returns the <typeparamref name="T"/> associated with this Id. Makes an API call to obtain information.
        /// </summary>
        /// <returns>A task that contains the <typeparamref name="T"/> instance upon completion.</returns>
        /// <remarks>Roblox's API is only invoked once. Any subsequent calls to this method will return a cached value. If the returned <typeparamref name="T"/> is a <see cref="IRefreshable"/>, take advantage of <see cref="IRefreshable.RefreshAsync"/> if you are expecting data to change.</remarks>
        public async Task<T> GetInstanceAsync(Session? session = null)
        {
            Session? sessionToUse = session ?? storedSession ?? GlobalSession.Assigned;
            stored ??= await T.FromId(Id, sessionToUse);
            return stored;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Id.ToString();
        }

        /// <summary>
        /// Converts the <see cref="GenericId{T}"/> to its Id. Equivalent to accessing <see cref="GenericId{T}.Id"/>.
        /// </summary>
        /// <param name="id">The <see cref="GenericId{T}"/> to convert.</param>
        public static implicit operator ulong(GenericId<T> id) => id.Id;
    }
}
