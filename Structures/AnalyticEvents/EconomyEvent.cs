using RoSharp.Enums;

namespace RoSharp.Structures.AnalyticEvents
{
    /// <summary>
    /// Analytic event representing a transaction involving the gain or loss of currency.
    /// </summary>
    public class EconomyEvent : AnalyticEvent
    {
        /// <summary>
        /// Gets the type of transaction that occurred.
        /// </summary>
        public EconomyEventFlowType FlowType { get; internal set; }

        /// <summary>
        /// Gets the new balance this user has of this particular currency.
        /// </summary>
        public int EndingBalance { get; internal set; }

        /// <summary>
        /// Gets the Item SKU that was purchased. Can be <see langword="null"/>.
        /// </summary>
        public string? ItemSKU { get; internal set; }

        /// <summary>
        /// Gets the transaction type.
        /// </summary>
        public string TransactionType { get; internal set; }

    }
}
