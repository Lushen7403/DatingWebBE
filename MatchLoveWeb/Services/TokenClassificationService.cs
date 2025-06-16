using System.Text.Json;
using System.Text;

namespace MatchLoveWeb.Services
{
    public class ToxicityResponse
    {
        public string level { get; set; }
        public int toxic_token_count { get; set; }
        public float density { get; set; }
        public bool has_heavy_word { get; set; }
    }

    public class TokenClassificationService
    {
        private readonly HttpClient _httpClient;

        public TokenClassificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ToxicityResponse?> ClassifyAsync(string text)
        {
            var payload = new { text = text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/moderate", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ToxicityResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
