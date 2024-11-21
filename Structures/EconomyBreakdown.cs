using RoSharp.Enums;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a breakdown of group income.
    /// </summary>
    public readonly struct EconomyBreakdown
    {
        /// <summary>
        /// The time-length that was used to retrieve the data.
        /// </summary>
        public AnalyticTimeLength TimeLength { get; }

        /// <summary>
        /// Gets a <see cref="System.TimeSpan"/> instance representing the length in <see cref="TimeLength"/>.
        /// </summary>
        public TimeSpan TimeSpan => TimeLength switch
        {
            AnalyticTimeLength.Day => TimeSpan.FromDays(1),
            AnalyticTimeLength.Week => TimeSpan.FromDays(7),
            AnalyticTimeLength.Month => TimeSpan.FromDays(31),
            AnalyticTimeLength.Year => TimeSpan.FromDays(365),
            _ => throw new UnreachableException("Invalid AnalyticTimeLength"),
        };

        /// <summary>
        /// Total amount of income.
        /// </summary>
        public int TotalIncome { get; }

        /// <summary>
        /// Total amount of pending Robux income.
        /// </summary>
        public int PendingRobux { get; }

        /// <summary>
        /// Breakdown of income, sorted by various <see cref="IncomeType"/>s.
        /// </summary>
        public ReadOnlyDictionary<IncomeType, int> Breakdown { get; }

        internal EconomyBreakdown(AnalyticTimeLength selected, int totalIncome, Dictionary<IncomeType, int> breakdown, int pendingRobux = 0)
        {
            TimeLength = selected;
            TotalIncome = totalIncome;
            Breakdown = breakdown.AsReadOnly();
            PendingRobux = pendingRobux;
        }
    }
}
