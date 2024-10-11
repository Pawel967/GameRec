using Game_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Game_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;

        public GameController()
        {
            _gameService = new GameService();
        }

        [HttpGet("{appId}")]
        public async Task<IActionResult> GetGameDetails(int appId)
        {
            var gameDetails = await _gameService.GetGameByAppIdAsync(appId);

            if (gameDetails == null)
            {
                return NotFound(new { Message = "Game not found in Steam Store." });
            }

            return Ok(gameDetails);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchGames([FromQuery] string name)
        {
            var games = await _gameService.SearchGamesByNameAsync(name);

            if (games == null || !games.Any())
            {
                return NotFound(new { Message = "No games found with the specified name." });
            }

            return Ok(games);
        }
    }
}
