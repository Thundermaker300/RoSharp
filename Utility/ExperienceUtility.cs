﻿using Newtonsoft.Json.Linq;
using RoSharp.Enums;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Structures;
using RoSharp.Structures.PurchaseTypes;
using System.Collections.ObjectModel;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for experiences.
    /// </summary>
    public static class ExperienceUtility
    {
        /// <summary>
        /// Gets the universe Id from a place Id.
        /// </summary>
        /// <param name="placeId">The place Id.</param>
        /// <returns>The universe Id.</returns>
        /// <exception cref="ArgumentException">Invalid place ID provided.</exception>
        public static async Task<HttpResult<ulong>> GetUniverseIdAsync(ulong placeId)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("apis")}/universes/v1/places/{placeId}/universe");
            HttpResponseMessage response = await HttpManager.SendAsync(null, payload);
            string raw = await response.Content.ReadAsStringAsync();
            dynamic universeData = JObject.Parse(raw);
            if (universeData.universeId != null)
            {
                return new(response, Convert.ToUInt64(universeData.universeId));
            }
            
            throw new ArgumentException("Invalid place ID provided.", nameof(placeId));
        }

        /// <summary>
        /// Gets a list of fiat (local currency) purchase options for experiences.
        /// </summary>
        /// <param name="session">An authenticated session, required.</param>
        /// <returns>A task containing a <see cref="ReadOnlyCollection{T}"/> of <see cref="FiatPurchase"/> upon completion.</returns>
        public static async Task<HttpResult<ReadOnlyCollection<FiatPurchase>>> GetFiatOptionsAsync(Session? session)
        {
            HttpMessage payload = new(HttpMethod.Get, $"{Constants.URL("apis")}/fiat-paid-access-service/v1/product/prices")
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetFiatOptionsAsync),
            };

            var response = await HttpManager.SendAsync(session.Global(), payload);
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            List<FiatPurchase> list = [];
            foreach (dynamic item in data.prices)
            {
                list.Add(new()
                {
                    Id = item.id,
                    CurrencyCode = item.amount.currencyCode,
                    Price = Convert.ToDouble(item.amount.units + "." + Convert.ToString(item.amount.nanos)),
                    PayoutAmount = Convert.ToDouble(item.payoutAmount.units + "." + Convert.ToString(item.payoutAmount.nanos)),
                    PayoutPercent = Convert.ToDouble(item.payoutPercentage),
                });
            }
            return new(response, list.AsReadOnly());
        }

        /// <summary>
        /// Returns a <see cref="Genre"/> given the name of the genre. This method is case-insensitive.
        /// </summary>
        /// <param name="genreName">The name of the genre.</param>
        /// <returns>The <see cref="Genre"/>.</returns>
        /// <exception cref="ArgumentException">Invalid genre name provided.</exception>
        /// <remarks>This method will automatically remove spaces and dashes (-), and will replace ampersand symbols with the word "And".</remarks>
        public static Genre GetGenre(string genreName)
        {
            if (genreName != null)
            {
                if (genreName == string.Empty)
                    return Genre.None;


                string newGenreName = genreName
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("&", "And");

                if (newGenreName.ToLower() == "1vsall") // Special case since enums can't start with #s.
                    return Genre.OneVsAll;

                if (Enum.TryParse(newGenreName, true, out Genre genre))
                    return genre;
            }

            throw new ArgumentException($"Unexpected genre type: {genreName}.");
        }

        internal static string ToInternalKey(Genre genre)
        {
            string str = string.Empty;

            // this one is a pain in the ###
            if (genre == Genre.TurnbasedRPG)
                return "turn_based_rpg";

            if (IsMainGenre(genre))
                str += "other_";

            foreach (char c in genre.ToString())
            {
                if (char.IsUpper(c) && str.Length != 0 && !str.EndsWith('_'))
                {
                    str += "_";
                }
                str += char.ToLower(c);
            }
            return str;
        }

        /// <summary>
        /// Gets whether or not the provided genre is not a subgenre.
        /// </summary>
        /// <param name="g">The genre.</param>
        /// <returns>True if it is a main genre, false if it is a subgenre.</returns>
        public static bool IsMainGenre(Genre g) => (int)g is > 0 and < 18;
    }
}
