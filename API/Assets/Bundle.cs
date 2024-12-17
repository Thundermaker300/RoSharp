/*using RoSharp.API.Pooling;
using RoSharp.Extensions;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets
{
    /// <summary>
    /// Represents a Roblox bundle. NOT YET IMPLEMENTED, DO NOT USE.
    /// </summary>
    /// <exception cref="NotImplementedException">Not implemented.</exception>
    public class Bundle : Asset, IRefreshable, IIdApi<Bundle>
    {
        private Bundle(ulong assetId, Session? session = null)
            : base(assetId, session) { }

        /// <summary>NOT YET IMPLEMENTED, DO NOT USE.</summary>
        /// <exception cref="NotImplementedException">Not implemented.</exception>
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
        public new Bundle AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
*/