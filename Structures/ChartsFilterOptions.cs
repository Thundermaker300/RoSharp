using RoSharp.Enums;

namespace RoSharp.Structures
{
    /// <summary>
    /// Options to filter <see cref="API.GameAPI.GetFrontPageExperiencesAsync"/> by.
    /// </summary>
    public class ChartsFilterOptions
    {
        /// <summary>
        /// Indicates whether or not to include experiences that require "high end" devices, such as newer phones.
        /// </summary>
        /// <remarks>Only applies to <see cref="Device.Tablet"/> and <see cref="Device.Phone"/>.</remarks>
        public bool IsHighEndDevice { get; set; }

        /// <summary>
        /// Indicates the device that experiences must be available on.
        /// </summary>
        public Device? Device { get; set; }

        /// <summary>
        /// Indicates the country that experiences must be available in. Expects a two-digit country code such as "us" for United States, "br" for Brazil, etc.
        /// </summary>
        public string? CountryCode { get; set; }
    }
}
