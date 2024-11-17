using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
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
