using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Represents an experience's thumbnail.
    /// </summary>
    public struct ExperienceThumbnail
    {
        /// <summary>
        /// Gets the asset that is this thumbnail.
        /// </summary>
        public Asset Asset { get; init; }

        /// <summary>
        /// Gets the alt-text for the thumbnail. Can be <see langword="null"/>.
        /// </summary>
        public string? AltText { get; init; }

        /// <summary>
        /// Gets the image URL of the thumbnail. Equivalent to <see cref="Asset.ThumbnailUrl"/>.
        /// </summary>
        public string ImageUrl => Asset.ThumbnailUrl;

        /// <summary>
        /// Gets an image URL in a specific size. Equivalent to <see cref="Asset.GetThumbnailAsync(ThumbnailSize)"/>.
        /// </summary>
        /// <param name="size">The size to use.</param>
        /// <returns>A task containing the URL upon completion.</returns>
        public async Task<string> GetImageUrlAsync(ThumbnailSize size = ThumbnailSize.S420x420) => await Asset.GetThumbnailAsync(size);
    }
}
