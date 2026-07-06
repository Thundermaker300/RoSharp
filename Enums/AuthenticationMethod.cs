using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates how a session was authenticated.
    /// </summary>
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Session is not authenticated.
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// Authenticated with a .ROBLOSECURITY token.
        /// </summary>
        RobloSecurity,

        /// <summary>
        /// Authenticated with an API key.
        /// </summary>
        ApiKey,
    }
}
