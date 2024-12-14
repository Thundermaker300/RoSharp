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
    /// Indicates that an item is free.
    /// </summary>
    public struct FreePurchase : IPurchaseType
    {
        /// <inheritdoc/>
        public PurchaseType Type => PurchaseType.Free;

        /// <inheritdoc/>
        public double Price => 0;

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[FREE]";
        }
    }
}
