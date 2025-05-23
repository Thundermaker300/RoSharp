using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Communities.Forum
{
    /// <summary>
    /// Represents an emote that can be used in post reactions (and maybe text in the future?)
    /// </summary>
    public class ForumEmote
    {
        /// <summary>
        /// Gets the name of the set the emote is associated with.
        /// </summary>
        public string SetName { get; init; }

        /// <summary>
        /// Gets the unique Id of the set the emote is associated with.
        /// </summary>
        public string SetId { get; init; }

        /// <summary>
        /// Gets the name of the emote.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets the unique Id of the emote.
        /// </summary>
        public string Id { get; init; }

        internal ForumEmote() { }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ForumEmote {Name} [{Id}] {{SET:{SetName} [{SetId}]}}";
        }
    }
}
