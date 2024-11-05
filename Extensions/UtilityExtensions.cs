using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
