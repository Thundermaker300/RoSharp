using RoSharp.API.Assets;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    /// <summary>
    /// Represents an Id corresponding to the given <typeparamref name="T"/>. This class is created instead of <typeparamref name="T"/> directly in large API requests to avoid Roblox ratelimits.
    /// </summary>
    public class GenericId<T>
        where T: IIdApi<T>
    {
        private T? stored;
        private Session? storedSession;

        /// <summary>
        /// Creates a new <see cref="GenericId{T}"/> with the given Id.
        /// </summary>
        /// <param name="id">The user Id.</param>
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
        /// <returns>A task that contains the <typeparamref name="T"/> upon completion.</returns>
        /// <remarks>Roblox's API is only invoked once. Any subsequent calls to this method will return a cached value.</remarks>
        public async Task<T> GetInstanceAsync(Session? session = null)
        {
            Session? sessionToUse = session ?? storedSession;
            if (stored == null)
            {
                stored = await (Task<T>)(typeof(T).GetMethod("FromId").Invoke(null, new object[] { Id, sessionToUse }));
            }
            return stored;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public static implicit operator ulong(GenericId<T> id) => id.Id;
    }
}
