using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Represents a type of purchase on the Roblox website.
    /// </summary>
    public interface IPurchaseType
    {
        /// <summary>
        /// Gets the type of purchase.
        /// </summary>
        public PurchaseType PurchaseType { get; }

        /// <summary>
        /// Gets the price of the purchase.
        /// </summary>
        public double Price { get; }
    }
}
