using RoSharp.Enums;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Represents feedback provided on an experience.
    /// </summary>
    public readonly struct ExperienceReview
    {
        /// <summary>
        /// Gets the unique Id of the feedback.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets whether the feedback was positive or negative.
        /// </summary>
        public bool Positive { get; init; }

        /// <summary>
        /// Gets the <see cref="Id{T}"/> of the experience the feedback was targeted for.
        /// </summary>
        public Id<Experience> TargetExperience { get; init; }

        /// <summary>
        /// Gets the live version of the experience when the feedback was published.
        /// </summary>
        public int TargetVersion { get; init; }

        /// <summary>
        /// Gets the message of the feedback.
        /// </summary>
        public string Comment { get; init; }

        /// <summary>
        /// Gets a <see cref="Enums.Device"/> representing the device the feedback was posted on.
        /// </summary>
        public Device DeviceType { get; init; }

        /// <summary>
        /// Gets the OS of the <see cref="DeviceType"/> that was used.
        /// </summary>
        public string DeviceOS { get; init; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the creation date of the feedback.
        /// </summary>
        public DateTime Created { get; init; }
    }
}
