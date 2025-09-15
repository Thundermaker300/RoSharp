using RoSharp.Enums;
using RoSharp.Http;
using RoSharp.Structures;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Represents a live game server.
    /// </summary>
    public readonly struct GameServer
    {
        internal readonly Place place { get; init; }

        /// <summary>
        /// The unique Id of the server.
        /// </summary>
        public string ServerId { get; init; }

        /// <summary>
        /// The current amount of players playing on this server.
        /// </summary>
        public int Playing { get; init; }

        /// <summary>
        /// The maximum amount of players that can be in this server concurrently.
        /// </summary>
        public int MaxPlayers { get; init; }

        /// <summary>
        /// The average ping from all users within the place server.
        /// </summary>
        public int AveragePing { get; init; }

        /// <summary>
        /// The average FPS from all users within the place server.
        /// </summary>
        public double AverageFps { get; init; }

        /// <summary>
        /// Gets a URL that can be used to directly join this server.
        /// </summary>
        public string JoinUrl => $"https://www.roblox.com/games/{place.Id}/game?placeId={place.Id}&jobId={ServerId}";

        /// <summary>
        /// Shuts down this server.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task<HttpResult> ShutdownAsync()
        {
            HttpMessage message = new(HttpMethod.Post, $"/matchmaking-api/v1/game-instances/shutdown", new
            {
                placeId = place.Id,
                gameId = ServerId,
            })
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ShutdownAsync),
            };

            return new(await place.SendAsync(message, Constants.URL("apis")));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"GameServer {ServerId} {{PLACE:{place.Id}}} [{Playing}/{MaxPlayers}] [PING: {AveragePing}] [FPS: {AverageFps}]";
        }
    }
}
