using RoSharp.Structures.AnalyticEvents;

namespace RoSharp.Enums
{
    /// <summary>
    /// Represents different types of analytic events.
    /// </summary>
    public enum AnalyticEventType
    {
        /// <inheritdoc cref="ProgressionEvent" />
        ProgressionEvents,

        /// <inheritdoc cref="EconomyEvent" />
        EconomyEvents,

        /// <inheritdoc cref="CustomEvent" />
        CustomEvents,
    }
}
