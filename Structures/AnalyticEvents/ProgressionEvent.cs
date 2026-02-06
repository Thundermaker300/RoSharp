using RoSharp.Enums;
using RoSharp.Interfaces;

namespace RoSharp.Structures.AnalyticEvents
{
    /// <summary>
    /// Analytic event representing progression along a specific funnel/path.
    /// </summary>
    public class ProgressionEvent : AnalyticEvent
    {
        /// <summary>
        /// Gets the type of the funnel.
        /// </summary>
        public string FunnelType { get; internal set; }

        /// <summary>
        /// Gets the name of the current step.
        /// </summary>
        public string StepName { get; internal set; }

        /// <summary>
        /// Gets the session Id of the current funnel. This should be unique to all funnel sessions.
        /// </summary>
        public string FunnelSessionId { get; internal set; }

    }
}
