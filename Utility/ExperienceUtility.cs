using RoSharp.Enums;

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
