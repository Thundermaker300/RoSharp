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

    public class EnumerableHttpResult<T> : HttpResult<T>, IEnumerable
        where T : IEnumerable
    {

        public static implicit operator T(EnumerableHttpResult<T> result) => result.Value;

        internal EnumerableHttpResult(HttpResponseMessage response, T value) : base(response, value) { }

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
