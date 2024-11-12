using RoSharp.Enums;

namespace RoSharp.Extensions
{
    public static class UtilityExtensions
    {
        public static int Limit(this FixedLimit fixedLimit)
        {
            return Convert.ToInt32(fixedLimit.ToString().Replace("Limit", string.Empty));
        }
    }
}
