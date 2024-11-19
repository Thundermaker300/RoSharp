using RoSharp.API;
using System.Collections.ObjectModel;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents the response for the Charts API.
    /// </summary>
    public sealed class ChartsResponse
    {
        /// <summary>
        /// Gets the token that can be used in the cursor parameter of <see cref="ChartsAPI.GetFrontPageExperiencesAsync(Session?, string?)"/> to advance to the next page.
        /// </summary>
        /// <remarks>Will be <see langword="null"/> if it is already on the last page.</remarks>
        public string? NextPageToken { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="ChartCategory"/> experiences within this request.
        /// </summary>
        public ReadOnlyCollection<ChartCategory> Categories { get; init; }

        internal ChartsResponse() { }
    }
}
