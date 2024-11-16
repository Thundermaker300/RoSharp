using RoSharp.API.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    public interface IIdApi<T>
    {
        public ulong Id { get; }
        public static Task<T> FromId(ulong id, Session? session) => throw new NotImplementedException();

        public T AttachSessionAndReturn(Session? session);
    }
}
