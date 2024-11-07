using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    public interface IAssetOwner
    {
        public string Name { get; }
        public ulong Id { get; }
    }
}
