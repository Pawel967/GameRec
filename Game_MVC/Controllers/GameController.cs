using Game_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Game_MVC.Controllers
{
    public class GameController : Controller
    {
        private readonly HttpClient _httpClient;

        public GameController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("GameApiClient");
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string gameName)
        {
            if (string.IsNullOrEmpty(gameName))
            {
                return View(new List<GameViewModel>());
            }

            var response = await _httpClient.GetAsync($"game/search?name={gameName}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var games = JsonConvert.DeserializeObject<List<GameViewModel>>(jsonData);
                return View(games);
            }

            return View(new List<GameViewModel>());
        }

        public async Task<IActionResult> Details(int appId)
        {
            var response = await _httpClient.GetAsync($"game/{appId}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var game = JsonConvert.DeserializeObject<GameViewModel>(jsonData);
                return View(game);
            }

            return NotFound();
        }
    }
}
