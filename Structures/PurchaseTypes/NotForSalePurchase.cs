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
    /// Indicates that an item is not for sale.
    /// </summary>
    public struct NotForSalePurchase : IPurchaseType
    {
        /// <inheritdoc/>
        public PurchaseType Type => PurchaseType.NotForSale;

        /// <inheritdoc/>
        public double Price => 0;

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[NOT FOR SALE]";
        }
    }
}
