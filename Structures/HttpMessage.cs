using System;
using System.Collections.Generic;
using System.Linq;
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
