using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    public interface IIdApi<T>
    {
        public static Task<T> FromId(ulong id, Session? session) => throw new NotImplementedException();
    }
}
