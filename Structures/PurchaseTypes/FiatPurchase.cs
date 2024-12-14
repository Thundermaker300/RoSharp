using RoSharp.Enums;
using RoSharp.Interfaces;

namespace RoSharp.Structures.PurchaseTypes
{
    /// <summary>
    /// Represents a fiat purchase type, which is a real-world currency purchase.
    /// </summary>
    public struct FiatPurchase : IPurchaseType
    {
        /// <summary>
        /// The unique Id of the fiat item.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// The currency code of the cost, typically <c>USD</c>.
        /// </summary>
        public string CurrencyCode { get; init; }

        /// <summary>
        /// The upfront cost of the item.
        /// </summary>
        public double Price { get; init; }

        /// <summary>
        /// The amount of payout to the developer using the item.
        /// </summary>
        public double PayoutAmount { get; init; }

        /// <summary>
        /// The percent of the purchase that goes to the developer.
        /// </summary>
        public double PayoutPercent { get; init; }

        /// <inheritdoc/>
        public PurchaseType Type => PurchaseType.LocalCurrency;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"({Price} {CurrencyCode})";
        }
    }
}
