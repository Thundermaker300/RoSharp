using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Represents different tabs in the private messaging system.
    /// </summary>
    public enum MessagesPageTab
    {
        /// <summary>
        /// Messages sent to the user.
        /// </summary>
        Inbox,

        /// <summary>
        /// Messages sent from the user.
        /// </summary>
        Sent,

        /// <summary>
        /// News messages sent to everyone from Roblox.
        /// </summary>
        News,

        /// <summary>
        /// Messages archived by the user.
        /// </summary>
        Archive,
    }
}
