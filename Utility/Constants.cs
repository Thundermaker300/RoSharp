using RoSharp.Enums;
using System.Collections.ObjectModel;

namespace RoSharp
{
    /// <summary>
    /// Constant information that doesn't change.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Contains the current Roblox URL.
        /// </summary>
        public const string ROBLOX_URL = "https://roblox.com";

        /// <summary>
        /// Contains a formatting URL used in <see cref="URL(string)"/>.
        /// </summary>
        public const string FORMAT_URL = "https://{0}.roblox.com";

        /// <summary>
        /// Contains a mapping of strings returned by the experience age recommendation API, mapped to the matching <see cref="ExperienceDescriptorType"/>.
        /// </summary>
        public static readonly ReadOnlyDictionary<string, ExperienceDescriptorType> DescriptorIdToEnumMapping = new Dictionary<string, ExperienceDescriptorType>()
        {
            ["alcohol"] = ExperienceDescriptorType.Alcohol,
            ["blood"] = ExperienceDescriptorType.Blood,
            ["crude-humor"] = ExperienceDescriptorType.CrudeHumor,
            ["fear"] = ExperienceDescriptorType.Fear,
            ["gambling"] = ExperienceDescriptorType.Gambling,
            ["romance"] = ExperienceDescriptorType.Romance,
            ["strong-language"] = ExperienceDescriptorType.StrongLanguage,
            ["social-hangout"] = ExperienceDescriptorType.SocialHangout,
            ["violence"] = ExperienceDescriptorType.Violence,
        }.AsReadOnly();

        /// <summary>
        /// Returns the Roblox API URL for the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The subdomain of the url, eg. 'catalog' for 'catalog.roblox.com'.</param>
        /// <returns>String URL</returns>
        public static string URL(string key) => string.Format(FORMAT_URL, key);
    }
}
