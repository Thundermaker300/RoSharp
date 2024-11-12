using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    public interface IRefreshable
    {
        public DateTime RefreshedAt { get; set; }
        public Task RefreshAsync();
    }
}
