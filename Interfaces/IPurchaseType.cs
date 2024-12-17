using RoSharp.Enums;
using RoSharp.Structures.PurchaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Represents a type of purchase on the Roblox website.
    /// <para>
    /// Casts to the following structs:<br/>
    /// * <see cref="FiatPurchase"/> - For purchases with local currency, defined in USD.<br/>
    /// * <see cref="FreePurchase"/> - For purchases that are free!<br/>
    /// * <see cref="NotForSalePurchase"/> - Placeholder for purchases that are not currently for sale.<br/>
    /// * <see cref="RobuxPurchase"/> - For purchases with Robux.<br/>
    /// * <see cref="UnknownPurchase"/> - Unknown purchase type, typically shouldn't be possible.
    /// </para>
    /// </summary>
    /// <example>
    /// <example>
    /// This code shows how to check an item's Robux price.
        /// <code>
        /// int robuxPrice = asset.PurchaseInfo is RobuxPurchase robux ? robux.Price : 0;
        /// </code>
    /// This code shows how to see if an item is on sale.
        /// <code>
        /// bool onSale = asset.PurchaseInfo is not NotForSalePurchase;
        /// </code>
    /// </example>
    public interface IPurchaseType
    {
        /// <summary>
        /// Gets the type of purchase.
        /// </summary>
        public PurchaseType Type { get; }

        /// <summary>
        /// Gets the price of the purchase.
        /// </summary>
        public double Price { get; }
    }
}
