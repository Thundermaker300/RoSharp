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
        /// Unknown avatar type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Represents a player rig with 6 joints.
        /// </summary>
        R6,

        /// <summary>
        /// Represents a player rig with 15 joints.
        /// </summary>
        R15,

        /// <summary>
        /// Allow users to choose what character rig they use.
        /// </summary>
        PlayerChoice,
    }
}
