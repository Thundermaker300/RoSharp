using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class UserId
    {
        private User user;

        public UserId(ulong id) => Id = id;

        public ulong Id { get; }

        /// <summary>
        /// Returns the <see cref="User"/> associated with this ID. Makes an API call to obtain user information.
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
