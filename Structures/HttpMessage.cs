using RoSharp.Enums;
using RoSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a simplicated <see cref="HttpRequestMessage"/> that can be re-used.
    /// </summary>
    public struct HttpMessage
    {
        /// <summary>
        /// Gets or sets the method used in the request.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the full URL used in the request.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the content sent in the request. Ignored for <c>GET</c> and <c>DELETE</c> requests.
        /// </summary>
        public object? Content { get; set; }

        /// <summary>
        /// Gets or sets additional headers to send in the request.
        /// <para>
        /// The following headers are automatically provided by RoSharp and will be ignored if added here:
        /// COOKIE, X-CSRF-TOKEN, X-API-KEY, CONTENT-TYPE, CONTENT-LENGTH
        /// </para>
        /// </summary>
        public Dictionary<string, IEnumerable<string>> Headers { get; set; } = [];

        /// <summary>
        /// Gets or sets the authentication type required to make this request. Defaults to <see cref="AuthType.None"/>.
        /// </summary>
        public AuthType AuthType { get; set; } = AuthType.None;

        /// <summary>
        /// Gets or sets the name of the API to throw back if the <see cref="AuthType"/> is not met.
        /// </summary>
        public string ApiName { get; set; }

        /// <summary>
        /// Gets or sets the name of the API Key permission to throw back if <see cref="AuthType"/> is <see cref="AuthType.ApiKey"/> and not met.
        /// </summary>
        public string ApiKeyPermission { get; set; }

        /// <summary>
        /// Gets or sets whether or not to enable retrying on this request. Defaults to <see langword="true"/>. Will not apply to <c>GET</c> and <c>DELETE</c> requests.
        /// <para>
        /// The request will ONLY be retried if the result is <see cref="HttpStatusCode.Forbidden"/>.
        /// </para>
        /// </summary>
        /// <remarks>Retrying is useful as the first failed request will obtain the X-CSRF-TOKEN and send it with the second request.</remarks>
        public bool EnableRetrying { get; set; } = true;

        /// <summary>
        /// Gets or sets whether or not to force an X-CSRF-TOKEN retry, completely ignoring the value of <see cref="EnableRetrying"/>. This will retry to retrieve the X-CSRF-TOKEN regardless of the type of request and error code.
        /// </summary>
        public bool ForceXCSRFRetry { get; set; }

        /// <summary>
        /// Gets or sets whether to silence <see cref="RobloxAPIException"/>s from this request.
        /// </summary>
        public bool SilenceExceptions { get; set; }

        /// <summary>
        /// Creates a new <see cref="HttpMessage"/>.
        /// </summary>
        /// <param name="method">The method to use.</param>
        /// <param name="url">The URL to send to.</param>
        /// <param name="content">The content of the message. Optional and not used for <c>GET</c> and <c>DELETE</c> requests.</param>
        public HttpMessage(HttpMethod method, string url, object? content = null)
        {
            Method = method;
            Url = url;
            Content = content;
        }
    }
}
