using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a config value for an experience.
    /// </summary>
    public readonly struct ExperienceConfig
    {
        /// <summary>
        /// The config key.
        /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// The config value.
        /// </summary>
        public string Value { get; init; }

        /// <summary>
        /// A description for the config.
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// The current draft hash the config is apart of.
        /// </summary>
        public string DraftHash { get; init; }
    }
}
