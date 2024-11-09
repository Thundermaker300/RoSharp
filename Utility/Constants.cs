using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Utility
{
    public static class Constants
    {
        public static readonly Dictionary<string, ExperienceDescriptors> DescriptorIdToEnumMapping = new()
        {
            ["alcohol"] = ExperienceDescriptors.Alcohol,
            ["blood"] = ExperienceDescriptors.Blood,
            ["crude-humor"] = ExperienceDescriptors.CrudeHumor,
            ["fear"] = ExperienceDescriptors.Fear,
            ["gambling"] = ExperienceDescriptors.Gambling,
            ["romance"] = ExperienceDescriptors.Romance,
            ["strong-language"] = ExperienceDescriptors.StrongLanguage,
            ["social-hangout"] = ExperienceDescriptors.SocialHangout,
            ["violence"] = ExperienceDescriptors.Violence,
        };
    }
}
