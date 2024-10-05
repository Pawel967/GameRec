using Game_API.Clients;
using Game_API.Dtos;

namespace Game_API.Services
{
    public class GameService
    {
        private readonly SteamApiClient _steamApiClient;

        public GameService()
        {
            _steamApiClient = new SteamApiClient();
        }

        public async Task<GameDetails?> GetGameByAppIdAsync(int appId)
        {
            return await _steamApiClient.GetGameDetailsAsync(appId);
        }

        public async Task<List<GameDetails>> SearchGamesByNameAsync(string gameName)
        {
            return await _steamApiClient.SearchGamesByNameAsync(gameName);
        }
    }
}
