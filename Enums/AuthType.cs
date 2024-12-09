using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Represents the type of authentication to use for an HTTP request.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// No auth necessary.
        /// </summary>
        None,

        /// <summary>
        /// Authenticated account via RobloSecurity.
        /// </summary>
        RobloSecurity,

        /// <summary>
        /// Open Cloud API key.
        /// </summary>
        ApiKey,

        /// <summary>
        /// Authenticated account via RobloSecurity AND an Open Cloud API key.
        /// </summary>
        RobloSecurityAndApiKey,
    }
}
