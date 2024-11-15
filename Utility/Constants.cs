using RoSharp.Enums;
using System.Collections.ObjectModel;

namespace RoSharp.Utility
{
    public static class Constants
    {
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
