using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates an experience's avatar type setting.
    /// </summary>
    public enum AvatarType
    {
        /// <summary>
        /// Unknown avatar type within this experience.
        /// </summary>
        Unknown,

        /// <summary>
        /// Set all users to R6 avatars.
        /// </summary>
        MorphToR6,

        /// <summary>
        /// Set all users to R15 or Rthro avatars.
        /// </summary>
        MorphToR15,

        /// <summary>
        /// Allow users to choose what character rig they use.
        /// </summary>
        PlayerChoice,
    }
}
