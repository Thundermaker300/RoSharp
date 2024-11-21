using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates an experience activity history type. Used in <see cref="API.Assets.Experiences.DeveloperStats.GetActivityHistoryAsync"/>.
    /// </summary>
    public enum ExperienceActivityHistoryType
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
        /// The experience's name was modified.
        /// </summary>
        NameUpdated = 16,

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
        /// The experience's playable devices was modified.
        /// </summary>
        PlayableDevicesModified = 70,

        /// <summary>
        /// The experience's private servers enabled setting was modified.
        /// </summary>
        PrivateServersModified = 71,

        /// <summary>
        /// The experience's private server price was modified.
        /// </summary>
        PrivateServerPriceModified = 72,

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
