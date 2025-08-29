namespace RoSharp.Enums
{
    /// <summary>
    /// Represents a social media channel allowed by Roblox for experiences, users, communities, or assets.
    /// </summary>
    public enum SocialMedia
    {
        /// <summary>
        /// Link to facebook.com. Applicable for: Assets, Communities, Experiences, Users.
        /// </summary>
        Facebook = 1,

        /// <summary>
        /// Link to x.com (formerly twitter.com). Still referred to as Twitter internally. Applicable for: Assets, Communities, Experiences, Users.
        /// </summary>
        Twitter = 2,

        /// <summary>
        /// Link to youtube.com. Applicable for: Assets, Communities, Experiences, Users.
        /// </summary>
        YouTube = 3,

        /// <summary>
        /// Link to twitch.tv. Applicable for: Assets, Communities, Experiences, Users.
        /// </summary>
        Twitch = 4,

        /// <summary>
        /// Link to discord.com. Applicable for: Assets, Communities, Experiences.
        /// </summary>
        Discord = 5,

        /// <summary>
        /// Link to a repository on github.com. Applicable for: Assets.
        /// </summary>
        GitHub = 6,

        /// <summary>
        /// Link to guilded.gg. Applicable for: Assets, Communities, Experiences, Users.
        /// </summary>
        Guilded = 7,

        /// <summary>
        /// Link to a Roblox community or experience. Internally defined as RobloxGroup, but can be used as both for assets only.
        /// </summary>
        RobloxGroup = 8,

        /// <summary>
        /// Link to a post on devforum.roblox.com. Applicable for: Assets.
        /// </summary>
        DevForum = 9,

        /// <summary>
        /// Link to amazon.com. Still present internally but not available for use anywhere by devs unless enabled by Roblox on a product.
        /// </summary>
        Amazon = 20,
    }
}
