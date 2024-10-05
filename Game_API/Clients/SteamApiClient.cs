using Game_API.Dtos;
using RestSharp;
using System.Text.Json;

namespace Game_API.Clients
{
    public class SteamApiClient
    {
        private readonly string _steamApiBaseUrl = "https://store.steampowered.com/api/";
        private readonly string _reviewBaseUrl = "https://store.steampowered.com/appreviews/";
        private readonly RestClient _steamApiClient;
        private readonly RestClient _reviewClient;

        public SteamApiClient()
        {
            _steamApiClient = new RestClient(_steamApiBaseUrl);
            _reviewClient = new RestClient(_reviewBaseUrl);
        }

        // Get all needed game details
        public async Task<GameDetails?> GetGameDetailsAsync(int appId)
        {
            // Request for game details
            var gameDetailsRequest = new RestRequest($"appdetails?appids={appId}&cc=PL&currency=PLN", Method.Get);
            var gameDetailsResponse = await _steamApiClient.ExecuteAsync(gameDetailsRequest);

            if (gameDetailsResponse.IsSuccessful && gameDetailsResponse.Content != null)
            {
                var jsonResponse = JsonDocument.Parse(gameDetailsResponse.Content);
                var gameData = jsonResponse.RootElement.GetProperty(appId.ToString());

                if (gameData.GetProperty("success").GetBoolean())
                {
                    var data = gameData.GetProperty("data");

                    var gameDetails = new GameDetails
                    {
                        Name = data.GetProperty("name").GetString() ?? string.Empty,
                        Type = data.GetProperty("type").GetString() ?? string.Empty,
                        ShortDescription = data.GetProperty("short_description").GetString() ?? string.Empty,
                        HeaderImage = data.GetProperty("header_image").GetString() ?? string.Empty,
                        SteamAppID = data.GetProperty("steam_appid").GetInt32(),
                        IsFree = data.GetProperty("is_free").GetBoolean(),
                        ReleaseDate = data.GetProperty("release_date").GetProperty("date").GetString() ?? string.Empty,
                        Genres = data.TryGetProperty("genres", out var genresProp) ? genresProp.EnumerateArray().Select(x => x.GetProperty("description").GetString() ?? "").ToList() : new List<string>(),
                        Developers = data.TryGetProperty("developers", out var devProp) ? devProp.EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>(),
                        Publishers = data.TryGetProperty("publishers", out var pubProp) ? pubProp.EnumerateArray().Select(x => x.GetString() ?? "").ToList() : new List<string>(),
                        Categories = data.TryGetProperty("categories", out var catProp) ? string.Join(", ", catProp.EnumerateArray().Select(x => x.GetProperty("description").GetString())) : "",
                        Platform = data.TryGetProperty("platforms", out var platformsProp) ? string.Join(", ", platformsProp.EnumerateObject().Where(x => x.Value.GetBoolean()).Select(x => x.Name)) : "",
                    };

                    if (data.TryGetProperty("price_overview", out var priceProp))
                    {
                        gameDetails.PriceOverview = new PriceOverview
                        {
                            Currency = "PLN",
                            Initial = priceProp.GetProperty("initial").GetInt32(),
                            Final = priceProp.GetProperty("final").GetInt32(),
                            DiscountPercent = priceProp.GetProperty("discount_percent").GetInt32()
                        };
                    }

                    // Request for game reviews
                    var reviewRequest = new RestRequest($"{appId}?json=1&language=all&purchase_type=all", Method.Get);
                    var reviewResponse = await _reviewClient.ExecuteAsync(reviewRequest);

                    if (reviewResponse.IsSuccessful && reviewResponse.Content != null)
                    {
                        var reviewJson = JsonDocument.Parse(reviewResponse.Content);
                        if (reviewJson.RootElement.TryGetProperty("success", out var successProp) && successProp.GetInt32() == 1)
                        {
                            var reviewSummary = reviewJson.RootElement.GetProperty("query_summary");

                            gameDetails.ReviewSummary = new ReviewSummary
                            {
                                NumReviews = reviewSummary.GetProperty("num_reviews").GetInt32(),
                                ReviewScore = reviewSummary.GetProperty("review_score").GetInt32(),
                                ReviewScoreDesc = reviewSummary.GetProperty("review_score_desc").GetString() ?? string.Empty,
                                TotalPositive = reviewSummary.GetProperty("total_positive").GetInt32(),
                                TotalNegative = reviewSummary.GetProperty("total_negative").GetInt32(),
                                TotalReviews = reviewSummary.GetProperty("total_reviews").GetInt32()
                            };
                        }
                    }

                    return gameDetails;
                }
            }

            return null;
        }


        // Search games by name
        public async Task<List<GameDetails>> SearchGamesByNameAsync(string gameName)
        {
            var request = new RestRequest($"storesearch?term={Uri.EscapeDataString(gameName)}&l=english&cc=US", Method.Get);
            var response = await _steamApiClient.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var jsonResponse = JsonDocument.Parse(response.Content);
                if (jsonResponse.RootElement.TryGetProperty("items", out var items))
                {
                    var gameDetailsList = new List<GameDetails>();

                    foreach (var item in items.EnumerateArray())
                    {
                        int appId = item.GetProperty("id").GetInt32();
                        var detailedGame = await GetGameDetailsAsync(appId);

                        if (detailedGame != null)
                        {
                            gameDetailsList.Add(detailedGame);
                        }
                    }

                    return gameDetailsList;
                }
            }

            return new List<GameDetails>();
        }
    }
}
