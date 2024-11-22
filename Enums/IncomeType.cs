namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates a type of income for communities and users.
    /// </summary>
    // [TODO] Note: The documentation within this enum is likely incorrect. Honest to god,
    // some of these are estimations. The API does not return any exact values.
    // Look into when possible.
    public enum IncomeType
    {
        // Communities //

        /// <summary>
        /// Unknown.
        /// </summary>
        RecurringRobuxStipend,

        /// <summary>
        /// Robux gained from purchased items within a group.
        /// </summary>
        ItemSaleRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        PurchasedRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        TradeSystemRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        GroupPayoutRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        IndividualToGroupRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        AdjustmentRobux,

        /// <summary>
        /// Robux gained from Immersive Ads payouts.
        /// </summary>
        ImmersiveAdPayouts,

        /// <summary>
        /// Robux gained from Subscription payouts.
        /// </summary>
        SubscriptionPayouts,

        /// <summary>
        /// Unknown.
        /// </summary>
        SubscriptionClawbacks,

        /// <summary>
        /// Robux gained from third-party sales.
        /// </summary>
        CommissionRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        GroupAffiliatePayoutRobux,

        // Users //

        /// <summary>
        /// Robux gained from user sales.
        /// </summary>
        Sales,

        /// <summary>
        /// Robux lost from user purchases.
        /// </summary>
        Purchases,

        /// <summary>
        /// Robux gained by third-party sales.
        /// </summary>
        AffiliateSales,

        /// <summary>
        /// Robux gained from group payouts sent to a user.
        /// </summary>
        GroupPayouts,

        /// <summary>
        /// Robux gained from purchasing Robux with real currency.
        /// </summary>
        CurrencyPurchases,

        /// <summary>
        /// Robux gained from premium stipends.
        /// </summary>
        PremiumStipends,

        /// <summary>
        /// Robux gained from the Roblox trading system.
        /// </summary>
        TradeSystemEarnings,

        /// <summary>
        /// Robux sent in the Roblox trading system.
        /// </summary>
        TradeSystemCosts,

        /// <summary>
        /// Robux lost from ad purchases.
        /// </summary>
        AdSpend,

        /// <summary>
        /// Robux lost from Developer Exchange (DevEX)
        /// </summary>
        DeveloperExchange,

        /// <summary>
        /// Unknown.
        /// </summary>
        IndividualToGroup,

        /// <summary>
        /// Presumed to be Robux gained from a Customer Service adjustment, but currently unknown.
        /// </summary>
        CSAdjustment,

        /// <summary>
        /// Unknown.
        /// </summary>
        AdsRevsharePayouts,

        /// <summary>
        /// Unknown.
        /// </summary>
        GroupAdsRevsharePayouts,

        /// <summary>
        /// Unknown.
        /// </summary>
        SubscriptionsRevshare,

        /// <summary>
        /// Unknown.
        /// </summary>
        GroupSubscriptionsRevshare,

        /// <summary>
        /// Unknown.
        /// </summary>
        SubscriptionsRevshareOutgoing,

        /// <summary>
        /// Unknown.
        /// </summary>
        GroupSubscriptionsRevshareOutgoing,

        /// <summary>
        /// Unknown.
        /// </summary>
        AffiliatePayout,


        // Both //

        /// <summary>
        /// Robux gained from premium payouts from a user experience.
        /// </summary>
        PremiumPayouts,

        /// <summary>
        /// Robux gained from premium payouts from a group experience.
        /// </summary>
        GroupPremiumPayouts,

        /// <summary>
        /// Gained Robux that has not yet been processed.
        /// </summary>
        PendingRobux,

        /// <summary>
        /// Unknown.
        /// </summary>
        PublishingAdvanceRebates,
    }
}
