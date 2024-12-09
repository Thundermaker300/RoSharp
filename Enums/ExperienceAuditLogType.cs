namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates an experience activity history type. Used in <see cref="API.Assets.Experiences.DeveloperStats.GetAuditLogsAsync"/>.
    /// </summary>
    // [TODO] Note: I'm sure there's some missing in here. I can only add what I can actually see.
    // if more are discovered please let me know.
    public enum ExperienceAuditLogType
    {
        /// <summary>
        /// The playability of the experience was modified.
        /// </summary>
        ModifyPlayability = 2,

        /// <summary>
        /// A notification string was created.
        /// </summary>
        CreateNotification = 3,

        /// <summary>
        /// A notification string was deleted.
        /// </summary>
        DeleteNotification = 4,

        /// <summary>
        /// A notification string was updated.
        /// </summary>
        UpdateNotification = 5,

        /// <summary>
        /// The experience's name was modified.
        /// </summary>
        ExperienceRenamed = 16,

        /// <summary>
        /// The experience's name was modified.
        /// </summary>
        [Obsolete("Use ExperienceRenamed")]
        NameUpdated = ExperienceRenamed,

        /// <summary>
        /// The experience's description was modified.
        /// </summary>
        DescriptionUpdated = 17,

        /// <summary>
        /// The experience's "Studio Access to API" setting was modified.
        /// </summary>
        StudioAccessToAPIModified = 20,

        /// <summary>
        /// The experience's game servers were shut down.
        /// </summary>
        ServersShutdown = 23,

        /// <summary>
        /// The experience's paid-access setting was modified.
        /// </summary>
        PaidAccessModified = 68,

        /// <summary>
        /// The experience's cost to play (in Robux) was modified.
        /// </summary>
        PaidAccessPriceModified = 69,

        /// <summary>
        /// The experience's playable devices was modified.
        /// </summary>
        PlayableDevicesModified = 70,

        /// <summary>
        /// The experience's private servers enabled setting was modified.
        /// </summary>
        PrivateServersModified = 71,

        /// <summary>
        /// The experience's private server price (in Robux) was modified.
        /// </summary>
        PrivateServerPriceModified = 72,

        /// <summary>
        /// A user was invited to play and/or modify an experience.
        /// </summary>
        UserInvited = 88,

        /// <summary>
        /// A group's role was invited to play an experience.
        /// </summary>
        GroupRoleInvited = 89,

        /// <summary>
        /// An invited user's permission was changed from play to edit or vice versa.
        /// </summary>
        UserPermissionsChanged = 90,

        /// <summary>
        /// A user was removed from being able to play and/or modify an experience.
        /// </summary>
        UserRemoved = 91,

        /// <summary>
        /// A group's role was removed from being able to play an experience.
        /// </summary>
        GroupRoleRemoved = 93,

        /// <summary>
        /// The experience was renamed.
        /// </summary>
        PlaceRenamed = 103,

        /// <summary>
        /// A place was added to an experience.
        /// </summary>
        PlaceAdded = 118,

        /// <summary>
        /// A place was removed from an experience.
        /// </summary>
        PlaceRemoved = 119,

        /// <summary>
        /// A place had a new version published.
        /// </summary>
        PublishPlace = 120,

        /// <summary>
        /// The experience's age guidelines was modified.
        /// </summary>
        ModifyExperienceAgeGuidelines = 121,

        /// <summary>
        /// The experience's private server price was changed from free to robux or vice versa.
        /// </summary>
        PrivateServersPriceSettingModified = 122,

        /// <summary>
        /// The experience's EditableMesh/EditableImage setting was modified.
        /// </summary>
        ModifyExperienceEditableAPIAllowed = 123,
    }
}
