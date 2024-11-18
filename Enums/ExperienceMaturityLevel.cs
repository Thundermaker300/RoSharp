using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates an experience's maturity level.
    /// </summary>
    public enum ExperienceMaturityLevel
    {
        /// <summary>
        /// Experience does not have a defined maturity level.
        /// </summary>
        Unknown,

        /// <summary>
        /// Experience maturity level is 'minimum' and the experience is available for all ages.
        /// </summary>
        Minimal,

        /// <summary>
        /// Experience maturity level is 'mild' and the experience is available for ages 9+.
        /// </summary>
        Mild,

        /// <summary>
        /// Experience maturity level is 'moderate' and the experience is available for ages 13+.
        /// </summary>
        Moderate,

        /// <summary>
        /// Experience maturity level is 'restricted' and the experience is available for ages 17+.
        /// </summary>
        Restricted,
    }
}
