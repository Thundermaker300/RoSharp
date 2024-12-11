using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the type of a relationship between two communities.
    /// </summary>
    public enum CommunityRelationship
    {
        /// <summary>
        /// Two groups that are considered "allies".
        /// </summary>
        Allies,

        /// <summary>
        /// Two groups that are considered "enemies".
        /// </summary>
        Enemies,
    }
}
