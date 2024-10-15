using Game_MVC.Dtos;
using System.Net.Http.Headers;

namespace Game_MVC.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClientWithAuth()
        {
            var client = _httpClientFactory.CreateClient("GameApiClient");
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var client = _httpClientFactory.CreateClient("GameApiClient");
            var response = await client.PostAsJsonAsync("api/Auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }

        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            var client = _httpClientFactory.CreateClient("GameApiClient");
            var response = await client.PostAsJsonAsync("api/Auth/register", registerDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<UserDto> GetUserInfoAsync(Guid userId)
        {
            var client = CreateClientWithAuth();
            var response = await client.GetAsync($"api/Auth/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<UserDto> UpdateUserInfoAsync(Guid userId, UpdateUserDto updateUserDto)
        {
            var client = CreateClientWithAuth();
            var response = await client.PutAsJsonAsync($"api/Auth/update/{userId}", updateUserDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
    }
}
