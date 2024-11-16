using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    /// <summary>
    /// Represents a user's Id. This class is created instead of <see cref="User"/> in large API requests to avoid Roblox ratelimits.
    /// </summary>
    public class UserId
    {
        private User user;

        /// <summary>
        /// Creates a new <see cref="UserId"/> with the given Id.
        /// </summary>
        /// <param name="id">The user Id.</param>
        public UserId(ulong id) => Id = id;

        /// <summary>
        /// Gets the Id of the user.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Returns the <see cref="User"/> associated with this Id. Makes an API call to obtain user information.
        /// </summary>
        /// <returns>A task that contains the <see cref="User"/> upon completion.</returns>
        public async Task<User> GetUserAsync(Session? session = null)
        {
            if (user == null)
            {
                user = await User.FromId(Id, session);
            }
            return user;
        }
    }
}
