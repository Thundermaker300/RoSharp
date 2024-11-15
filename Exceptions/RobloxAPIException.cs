using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Exceptions
{
    public class RobloxAPIException : Exception
    {
        internal HttpStatusCode code;
        public HttpStatusCode Code => code;

        public RobloxAPIException(string message = "", HttpStatusCode code = HttpStatusCode.OK) : base(message)
        {
            this.code = code;
        }
    }
}
