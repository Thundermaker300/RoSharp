using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Exceptions
{
    public class RobloxAPIException : HttpRequestException
    {
        public RobloxAPIException(string message) : base(message) { }
    }
}
