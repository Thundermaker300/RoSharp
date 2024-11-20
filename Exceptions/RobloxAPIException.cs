using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Exceptions
{
    /// <summary>
    /// Indicates an exception caused by a Roblox API request.
    /// </summary>
    public class RobloxAPIException : Exception
    {
        internal HttpStatusCode code;

        /// <summary>
        /// The HTTP Code of the error.
        /// </summary>
        public HttpStatusCode Code => code;

        /// <summary>
        /// Initializes a new <see cref="RobloxAPIException"/>.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="code">The HTTP status code.</param>
        public RobloxAPIException(string message = "", HttpStatusCode code = HttpStatusCode.OK) : base(message)
        {
            this.code = code;
        }
    }
}
