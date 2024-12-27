using RoSharp.Enums;
using RoSharp.Interfaces;

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
            return "FreePurchase [FREE]";
        }
    }
}
