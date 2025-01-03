﻿using RoSharp.Enums;
using RoSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures.PurchaseTypes
{
    /// <summary>
    /// Represents a Robux purchase type.
    /// </summary>
    public struct RobuxPurchase : IPurchaseType
    {
        /// <summary>
        /// The upfront cost of the item.
        /// </summary>
        public double Price { get; init; }

        /// <inheritdoc/>
        public PurchaseType Type => PurchaseType.Robux;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"RobuxPurchase [R${Price}]";
        }
    }
}
