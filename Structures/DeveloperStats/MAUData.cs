using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures.DeveloperStats
{
    public struct MAUData
    {
        public int TotalMonthlyUsers { get; init; }
        public ReadOnlyDictionary<string, int> ByCountry { get; init; }
        public ReadOnlyDictionary<string, int> ByAgeRange { get; init; }
        public ReadOnlyDictionary<Gender, int> ByGender { get; init; }
        public ReadOnlyDictionary<string, int> ByLocale { get; init; }

    }
}
