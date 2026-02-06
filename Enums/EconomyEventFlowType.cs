using RoSharp.Structures.AnalyticEvents;

namespace RoSharp.Enums
{
    /// <summary>
    /// Defines different types of economy events.
    /// </summary>
    /// <seealso cref="EconomyEvent"/>
    public enum EconomyEventFlowType
    {
        /// <summary>
        /// The user is earning a currency.
        /// </summary>
        Source,

        /// <summary>
        /// The user is spending a currency.
        /// </summary>
        Sink,
    }
}
