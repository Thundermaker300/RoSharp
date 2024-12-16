using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Pooling;
using RoSharp.Extensions;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// Represents a Roblox game-pass.
    /// </summary>
    public class Bundle : Asset, IRefreshable, IIdApi<Bundle>
    {
        private Bundle(ulong assetId, Session? session = null)
            : base(assetId, session) { }

        /// <inheritdoc/>
        public new static async Task<Bundle> FromId(ulong BundleId, Session? session = null)
        {
            throw new NotImplementedException();

            if (RoPool<Bundle>.Contains(BundleId))
                return RoPool<Bundle>.Get(BundleId, session.Global());

            Bundle newUser = new(BundleId, session.Global());
            newUser.assetTypeOverride = "bundle";
            await newUser.RefreshAsync();

            RoPool<Bundle>.Add(newUser);

            return newUser;
        }

        /// <inheritdoc/>
        public new async Task RefreshAsync()
        {
            await base.RefreshAsync();
        }

        /// <inheritdoc/>
        public Bundle AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
