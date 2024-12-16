namespace RoSharp.Enums
{
    /// <summary>
    /// Indicates the type of an asset.
    /// </summary>
    /// <seealso cref="API.Assets.Asset.AssetType"/>
    public enum AssetType
    {
        // "Body" isn't an official AssetType. However, it is included in the enum so that it can be included
        // in the Price floor APIs (GetPriceFloorsAsync and GetPriceFloorForTypeAsync)

        /// <summary>
        /// Represents a body asset.
        /// </summary>
        /// <remarks>This member isn't an official AssetType type, but is included here so that it is included in the <see cref="API.PriceFloorAPI"/>.</remarks>
        Body = -1,

        /// <summary>
        /// Unknown asset type.
        /// </summary>
        Unknown = 0,

        // Regular
        /// <summary>
        /// Represents an image asset.
        /// </summary>
        /// <seealso cref="Decal"/>
        Image = 1,

        /// <summary>
        /// Represents a classic T-Shirt.
        /// </summary>
        /// <seealso cref="TShirtAccessory"/>
        TShirt = 2,

        /// <summary>
        /// Represents an audio asset.
        /// </summary>
        Audio = 3,

        /// <summary>
        /// Represents a mesh asset.
        /// </summary>
        /// <seealso cref="MeshPart"/>
        Mesh = 4,

        /// <summary>
        /// Represents a Lua script.
        /// </summary>
        Lua = 5,

        /// <summary>
        /// Represents a hat.
        /// </summary>
        Hat = 8,

        /// <summary>
        /// Represents an individual place.
        /// </summary>
        Place = 9,

        /// <summary>
        /// Represents a model.
        /// </summary>
        Model = 10,

        /// <summary>
        /// Represents a classic shirt.
        /// </summary>
        /// <seealso cref="ShirtAccessory"/>
        Shirt = 11,

        /// <summary>
        /// Represents a classic pants.
        /// </summary>
        /// <seealso cref="PantsAccessory"/>
        Pants = 12,

        /// <summary>
        /// Represents a decal.
        /// </summary>
        /// <seealso cref="Image"/>
        Decal = 13,

        /// <summary>
        /// Represents a classic head.
        /// </summary>
        Head = 17,

        /// <summary>
        /// Represents a classic face.
        /// </summary>
        Face = 18,

        /// <summary>
        /// Represents a gear.
        /// </summary>
        Gear = 19,

        /// <summary>
        /// Represents a badge.
        /// </summary>
        Badge = 21,

        /// <summary>
        /// Represents an animation asset.
        /// </summary>
        Animation = 24,

        /// <summary>
        /// Represents a torso asset from a bundle.
        /// </summary>
        Torso = 27,

        /// <summary>
        /// Represents a right-arm asset from a bundle.
        /// </summary>
        RightArm = 28,

        /// <summary>
        /// Represents a left-arm asset from a bundle.
        /// </summary>
        LeftArm = 29,

        /// <summary>
        /// Represents a left-leg asset from a bundle.
        /// </summary>
        LeftLeg = 30,

        /// <summary>
        /// Represents a right-leg asset from a bundle.
        /// </summary>
        RightLeg = 31,

        /// <summary>
        /// Represents a classic package.
        /// </summary>
        Package = 32,

        /// <summary>
        /// Represents a game-pass.
        /// </summary>
        GamePass = 34,

        /// <summary>
        /// Represents a plugin.
        /// </summary>
        Plugin = 38,

        /// <summary>
        /// Represents a meshpart. Unsure if used.
        /// </summary>
        MeshPart = 40,

        /// <summary>
        /// Represents a hair accessory.
        /// </summary>
        HairAccessory = 41,

        /// <summary>
        /// Represents a face accessory.
        /// </summary>
        FaceAccessory = 42,

        /// <summary>
        /// Represents a neck accessory.
        /// </summary>
        NeckAccessory = 43,

        /// <summary>
        /// Represents a shoulder accessory.
        /// </summary>
        ShoulderAccessory = 44,

        /// <summary>
        /// Represents a front accessory.
        /// </summary>
        FrontAccessory = 45,

        /// <summary>
        /// Represents a back accessory.
        /// </summary>
        BackAccessory = 46,

        /// <summary>
        /// Represents a waist accessory.
        /// </summary>
        WaistAccessory = 47,

        /// <summary>
        /// Represents the climbing animation in an animation package.
        /// </summary>
        ClimbAnimation = 48,

        /// <summary>
        /// Represents the death animation in an animation package.
        /// </summary>
        DeathAnimation = 49,

        /// <summary>
        /// Represents the falling animation in an animation package.
        /// </summary>
        FallAnimation = 50,

        /// <summary>
        /// Represents the idle animation in an animation package.
        /// </summary>
        IdleAnimation = 51,

        /// <summary>
        /// Represents the jumping animation in an animation package.
        /// </summary>
        JumpAnimation = 52,

        /// <summary>
        /// Represents the running animation in an animation package.
        /// </summary>
        RunAnimation = 53,

        /// <summary>
        /// Represents the swimming animation in an animation package.
        /// </summary>
        SwimAnimation = 54,

        /// <summary>
        /// Represents the walking animation in an animation package.
        /// </summary>
        WalkAnimation = 55,

        /// <summary>
        /// Represents the pose animation in an animation package.
        /// </summary>
        PoseAnimation = 56,

        /// <summary>
        /// Represents an ear accessory.
        /// </summary>
        EarAccessory = 57,

        /// <summary>
        /// Represents an eye accessory.
        /// </summary>
        EyeAccessory = 58,

        /// <summary>
        /// Represents an individual emote animation.
        /// </summary>
        EmoteAnimation = 61,

        /// <summary>
        /// Represents a video asset.
        /// </summary>
        Video = 62,

        /// <summary>
        /// Represents a 3D T-Shirt accessory.
        /// </summary>
        TShirtAccessory = 64,

        /// <summary>
        /// Represents a 3D Shirt accessory.
        /// </summary>
        ShirtAccessory = 65,

        /// <summary>
        /// Represents a 3D Pants accessory.
        /// </summary>
        PantsAccessory = 66,

        /// <summary>
        /// Represents a 3D Jacket accessory.
        /// </summary>
        JacketAccessory = 67,

        /// <summary>
        /// Represents a 3D Sweater accessory.
        /// </summary>
        SweaterAccessory = 68,

        /// <summary>
        /// Represents a 3D Shorts accessory.
        /// </summary>
        ShortsAccessory = 69,

        /// <summary>
        /// Represents a left-shoe asset from a shoes bundle.
        /// </summary>
        LeftShoeAccessory = 70,

        /// <summary>
        /// Represents a right-shoe asset from a shoes bundle.
        /// </summary>
        RightShoeAccessory = 71,

        /// <summary>
        /// Represents a 3D Dress Skirt accessory.
        /// </summary>
        DressSkirtAccessory = 72,

        /// <summary>
        /// Represents a font-family.
        /// </summary>
        FontFamily = 73,

        /// <summary>
        /// Represents an eyebrow accessory.
        /// </summary>
        EyebrowAccessory = 76,

        /// <summary>
        /// Represents an eyelash accessory.
        /// </summary>
        EyelashAccessory = 77,

        /// <summary>
        /// Represents a mood animation.
        /// </summary>
        MoodAnimation = 78,

        /// <summary>
        /// Represents a dynamic head asset.
        /// </summary>
        DynamicHead = 79,
    }
}
