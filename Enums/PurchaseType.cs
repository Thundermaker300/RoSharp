using RoSharp.Interfaces;

namespace RoSharp.Enums
{
    /// <summary>
    /// Represents the type of purchase. Used in the <see cref="IPurchaseType"/> interface.
    /// </summary>
    public enum PurchaseType
    {
        /// <summary>
        /// Unknown purchase form.
        /// </summary>
        Unknown,

        /// <summary>
        /// Asset is not for sale.
        /// </summary>
        NotForSale,
        
        /// <summary>
        /// Asset is free.
        /// </summary>
        Free,

        /// <summary>
        /// Asset costs Robux.
        /// </summary>
        Robux,

        /// <summary>
        /// Asset costs fiat (local currency).
        /// </summary>
        LocalCurrency,
    }
}
