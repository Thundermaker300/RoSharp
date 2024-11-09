using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Pooling
{
    internal interface IPoolable
    {
        public ulong Id { get; }

        public IPoolable AttachSessionAndReturn(Session? session);
    }
}
