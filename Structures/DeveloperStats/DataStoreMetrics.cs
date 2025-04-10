namespace RoSharp.Structures.DeveloperStats
{
    /// <summary>
    /// Contains data regarding an experience's DataStore metrics.
    /// </summary>
    public readonly struct DataStoreMetrics
    {
        /// <summary>
        /// Gets the total amount of bytes that are used across all data stores within the experience.
        /// </summary>
        public long TotalBytes { get; init; }

        /// <summary>
        /// Gets the maximum amount of bytes that are can be used for this experience. Roblox scales this value up based on Concurrent Users (CCU) statistics.
        /// </summary>
        /// <seealso cref="API.Assets.Experiences.DeveloperStats.GetConcurrentUsersAsync"/>
        /// <seealso cref="API.Assets.Experiences.DeveloperStats.GetConcurrentUsersByDeviceAsync"/>
        public long MaximumBytes { get; init; }

        /// <summary>
        /// Gets the total amount of unique data stores used in this experience.
        /// </summary>
        public int DataStores { get; init; }

        /// <summary>
        /// Gets the total amount of data store keys used in this experience.
        /// </summary>
        public long Keys { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{TotalBytes}/{MaximumBytes} BYTES [# DATASTORES: {DataStores}] [# KEYS: {Keys}]";
        }
    }
}
