using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    public struct HttpMessage
    {
        public HttpMethod Method { get; set; }
        public string Url { get; set; }
        public object? Content { get; set; }

        public HttpMessage(HttpMethod method, string url, object? content = null)
        {
            Method = method;
            Url = url;
            Content = content;
        }

        public HttpMessage(string url) => Url = url;
    }
}
