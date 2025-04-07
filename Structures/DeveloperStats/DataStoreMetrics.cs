using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures.DeveloperStats
{
    public readonly struct DataStoreMetrics
    {
        public long TotalBytes { get; init; }
        public long MaximumBytes { get; init; }
        public int DataStores { get; init; }
        public long Keys { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{TotalBytes}/{MaximumBytes} BYTES [# DATASTORES: {DataStores}] [# KEYS: {Keys}]";
        }
    }
}
