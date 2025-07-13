using System.Collections;
using System.Collections.Generic;

namespace RoSharp.Http
{
    public class HttpResult
    {
        public HttpResponseMessage HttpResponse { get; internal set; }

        public static implicit operator HttpResponseMessage(HttpResult response) => response.HttpResponse;

        public HttpResult(HttpResponseMessage response)
        {
            HttpResponse = response;
        }
    }

    public class HttpResult<T> : HttpResult
    {
        public T Value { get; internal set; }

        public static implicit operator T(HttpResult<T> result) => result.Value;

        internal HttpResult(HttpResponseMessage response, T value) : base(response)
        {
            Value = value;
        }
    }
}
