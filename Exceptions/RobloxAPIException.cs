using System.Net;

namespace RoSharp.Exceptions
{
    /// <summary>
    /// Indicates an exception caused by a Roblox API request.
    /// </summary>
    public class RobloxAPIException : Exception
    {
        internal HttpStatusCode code;
        internal int? retryIn;

        /// <summary>
        /// The HTTP Code of the error.
        /// </summary>
        public HttpStatusCode Code => code;

        /// <summary>
        /// Gets whether or not this API exception is the result of Too Many Requests to the Roblox API.
        /// </summary>
        public bool IsTooManyRequests => Code is HttpStatusCode.TooManyRequests;

        /// <summary>
        /// If <see cref="IsTooManyRequests"/> is <see langword="true"/>, gets the amount of seconds until the request should be retried. Can be <see langword="null"/> if certain Roblox APIs do not provide it.
        /// </summary>
        public int? RetryIn => retryIn;

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
