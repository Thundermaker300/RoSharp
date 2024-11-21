using RoSharp.Enums;
using System.Collections.ObjectModel;

namespace RoSharp.Structures.DeveloperStats
{
    /// <summary>
    /// Represents data regarding an experience's Monthly Active Users (MAU).
    /// </summary>
    public readonly struct MAUData
    {
        /// <summary>
        /// Total amount of monthly active users.
        /// </summary>
        public int TotalMonthlyUsers { get; init; }

        /// <summary>
        /// Monthly active users by country.
        /// </summary>
        public ReadOnlyDictionary<string, int> ByCountry { get; init; }

        /// <summary>
        /// Monthly active users by age range.
        /// </summary>
        public ReadOnlyDictionary<string, int> ByAgeRange { get; init; }

        /// <summary>
        /// Monthly active users by gender.
        /// </summary>
        public ReadOnlyDictionary<Gender, int> ByGender { get; init; }

        /// <summary>
        /// Monthly active users by locale.
        /// </summary>
        public ReadOnlyDictionary<string, int> ByLocale { get; init; }

    }
}
