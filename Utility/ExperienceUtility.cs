using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Utility
{
    /// <summary>
    /// Static class that contains utility methods for experiences.
    /// </summary>
    public static class ExperienceUtility
    {
        /// <summary>
        /// Returns a <see cref="Genre"/> given the name of the genre. This method is case-insensitive.
        /// </summary>
        /// <param name="genreName">The name of the genre.</param>
        /// <returns>The <see cref="Genre"/>.</returns>
        /// <exception cref="ArgumentException">Invalid genre name provided.</exception>
        /// <remarks>This method will automatically remove spaces and dashes (-), and will replace "&" symbols with the word "And".</remarks>
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
    }
}
