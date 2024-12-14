using RoSharp.Enums;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures.PurchaseTypes
{
    /// <summary>
    /// The price of the item was unable to be determined.
    /// </summary>
    public struct UnknownPurchase : IPurchaseType
    {
        /// <inheritdoc/>
        public PurchaseType Type => PurchaseType.Unknown;

        /// <inheritdoc/>
        public double Price => 0;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"UNKNOWN COST";
        }
    }
}
