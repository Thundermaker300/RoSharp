using RoSharp.Enums;

namespace RoSharp.Extensions
{
    /// <summary>
    /// Utility extensions.
    /// </summary>
    public static class UtilityExtensions
    {
        /// <summary>
        /// Converts a <see cref="FixedLimit"/> to the appropriate string to use within the Roblox API.
        /// </summary>
        /// <param name="fixedLimit">The <see cref="FixedLimit"/> to convert.</param>
        /// <returns>Roblox-API compatible string.</returns>
        public static int Limit(this FixedLimit fixedLimit)
        {
            return Convert.ToInt32(fixedLimit.ToString().Replace("Limit", string.Empty));
        }
    }
}
