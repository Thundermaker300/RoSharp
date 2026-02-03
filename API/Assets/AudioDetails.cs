using RoSharp.Enums;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// A struct that contains metadata information about the an audio asset.
    /// </summary>
    public struct AudioDetails
    {
        /// <summary>
        /// Gets the AssetId of the model that this struct is referring to.
        /// </summary>
        public ulong AssetId { get; internal set; }

        /// <summary>
        /// Gets the type of audio.
        /// </summary>
        public AudioType AudioType { get; internal set; }

        /// <summary>
        /// Gets the album the music is from. Will be <see langword="null"/> if <see cref="AudioType"/> is not <see cref="AudioType.Music"/>.
        /// </summary>
        public string? Album { get; internal set; }

        /// <summary>
        /// Gets the genre of the music. Will be <see langword="null"/> if <see cref="AudioType"/> is not <see cref="AudioType.Music"/>.
        /// </summary>
        public string? Genre { get; internal set; }

        /// <summary>
        /// Gets the total duration in seconds of the audio.
        /// </summary>
        /// <remarks>Please note: Roblox rounds down to the nearest full second. As such, this property will be zero for audio that are less than 1 second long.</remarks>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Gets the artist of the audio. For <see cref="AudioType.Music"/> audio, this will be the name of the artist in real life. For <see cref="AudioType.SoundEffect"/> audio, this will be identical to the audio uploader's username.
        /// </summary>
        public string Artist { get; internal set; }

        /// <summary>
        /// Gets the official title of the audio. This generally is identical to the name of the asset.
        /// </summary>
        public string Title { get; internal set; }

        public override string ToString()
        {
            return $"AudioDetails {AssetId} || {AudioType} | Title: {Title} | Genre: {Genre} | Album: {Album} | Artist: {Artist} | Duration: {Duration}";
        }
    }
}
