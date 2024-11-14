using RoSharp.Enums;

namespace RoSharp.Utility
{
    public static class Constants
    {
        public static readonly Dictionary<string, ExperienceDescriptorType> DescriptorIdToEnumMapping = new()
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
        };
    }
}
