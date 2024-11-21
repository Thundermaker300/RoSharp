namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates a type of income for groups and users.
    /// </summary>
    public enum IncomeType
    {
        // Groups
        RecurringRobuxStipend,
        ItemSaleRobux,
        PurchasedRobux,
        TradeSystemRobux,
        GroupPayoutRobux,
        IndividualToGroupRobux,
        AdjustmentRobux,
        ImmersiveAdPayouts,
        SubscriptionPayouts,
        SubscriptionClawbacks,
        CommissionRobux,
        GroupAffiliatePayoutRobux,

        // Users
        Sales,
        Purchases,
        AffiliateSales,
        GroupPayouts,
        CurrencyPurchases,
        PremiumStipends,
        TradeSystemEarnings,
        TradeSystemCosts,
        AdSpend,
        DeveloperExchange,
        IndividualToGroup,
        CSAdjustment,
        AdsRevsharePayouts,
        GroupAdsRevsharePayouts,
        SubscriptionsRevshare,
        GroupSubscriptionsRevshare,
        SubscriptionsRevshareOutgoing,
        GroupSubscriptionsRevshareOutgoing,
        AffiliatePayout,


        // Both
        PremiumPayouts,
        GroupPremiumPayouts,
        PendingRobux,
        PublishingAdvanceRebates,
    }
}
