using RoSharp.API;
using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a private message on Roblox.
    /// </summary>
    public struct PrivateMessage
    {
        /// <summary>
        /// Gets the unique Id of the private message.
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        /// Gets the recipient of the message.
        /// </summary>
        public Id<User> Recipient { get; init; }

        /// <summary>
        /// Gets the sender of the message.
        /// </summary>
        public Id<User> Sender { get; init; }

        /// <summary>
        /// Gets the subject line of the message.
        /// </summary>
        public string Subject { get; init; }

        /// <summary>
        /// Gets the contents of the message.
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Gets the time the message was created.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// Gets whether or not this message is less than 3 days old.
        /// </summary>
        public readonly bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        /// <summary>
        /// Gets whether or not this message is a system message, usually sent by the Roblox account.
        /// </summary>
        public bool IsSystemMessage { get; init; }

        /// <summary>
        /// Gets whether or not the authenticated user has read this message.
        /// </summary>
        public bool IsRead { get; init; }

        /// <summary>
        /// Gets the tab the message is located in.
        /// </summary>
        public MessagesPageTab CurrentTab { get; init; }
    }
}
