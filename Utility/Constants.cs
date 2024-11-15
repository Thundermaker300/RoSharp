using RoSharp.Enums;
using System.Collections.ObjectModel;

namespace RoSharp.Utility
{
    public static class Constants
    {
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
    }
}
