using RoSharp.API;
using RoSharp.Enums;

namespace RoSharp.Structures.AnalyticEvents
{
    /// <summary>
    /// Base class that represents an analytic event. Can be casted to one of the three following types: <see cref="ProgressionEvent"/>, <see cref="EconomyEvent"/>, and <see cref="CustomEvent"/>.
    /// </summary>
    public class AnalyticEvent
    {
        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        public AnalyticEventType EventType { get; internal set; }

        /// <summary>
        /// Gets the Id of the user involved in the event.
        /// </summary>
        public Id<User> UserId { get; internal set; }

        /// <summary>
        /// Gets the name of the event.
        /// <br/>
        /// <br/>
        /// For <see cref="AnalyticEventType.ProgressionEvents"/>, this is the name of the funnel.
        /// <br/>
        /// For <see cref="AnalyticEventType.EconomyEvents"/>, this is the name of the currency.
        /// <br/>
        /// For <see cref="AnalyticEventType.CustomEvents"/>, this is the name of the custom event.
        /// </summary>
        public string EventName { get; internal set; }

        /// <summary>
        /// Gets the value of the event.
        /// <br/>
        /// <br/>
        /// For <see cref="AnalyticEventType.ProgressionEvents"/>, this is the current step on the funnel.
        /// <br/>
        /// For <see cref="AnalyticEventType.EconomyEvents"/>, this is the amount of currency gained/spent.
        /// <br/>
        /// For <see cref="AnalyticEventType.CustomEvents"/>, this is the value of the custom event.
        /// </summary>
        public int Value { get; internal set; }

        /// <summary>
        /// Gets the time the event occurred.
        /// </summary>
        public DateTime Time { get; internal set; }

        /// <summary>
        /// Gets the value of custom field 1. Can be <see langword="null"/>.
        /// </summary>
        public string? CustomField1 { get; internal set; }

        /// <summary>
        /// Gets the value of custom field 2. Can be <see langword="null"/>.
        /// </summary>
        public string? CustomField2 { get; internal set; }

        /// <summary>
        /// Gets the value of custom field 3. Can be <see langword="null"/>.
        /// </summary>
        public string? CustomField3 { get; internal set; }

    }
}
