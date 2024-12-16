using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Pooling;
using RoSharp.Extensions;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets
{
    public class GamePass : Asset, IRefreshable, IIdApi<GamePass>
    {
        private Id<Place> place;

        /// <summary>
        /// Gets the place this game pass is associated with.
        /// </summary>
        public Id<Place> Place => place;

        private GamePass(ulong assetId, Session? session = null)
            : base(assetId, session) { }

        public static async Task<GamePass> FromId(ulong gamepassId, Session? session = null)
        {
            if (RoPool<GamePass>.Contains(gamepassId))
                return RoPool<GamePass>.Get(gamepassId, session.Global());

            GamePass newUser = new(gamepassId, session.Global());
            newUser.assetTypeOverride = "gamepass";
            await newUser.RefreshAsync();

            RoPool<GamePass>.Add(newUser);

            return newUser;
        }

        public async Task RefreshAsync()
        {
            await base.RefreshAsync();
            string gamepassData = await SendStringAsync(HttpMethod.Get, $"/game-passes/v1/game-passes/{Id}/details", Constants.URL("apis"));
            dynamic data = JObject.Parse(gamepassData);

            ulong placeId = data.placeId;
            place = new(placeId, session);
        }

        /// <inheritdoc/>
        public GamePass AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
