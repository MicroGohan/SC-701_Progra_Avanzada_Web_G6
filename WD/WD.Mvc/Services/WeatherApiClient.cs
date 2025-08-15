using System.Text.Json;
using WD.Models.WDModels;

namespace WD.Mvc.Services
{
    public class WeatherApiClient
    {
        private readonly HttpClient _http;

        public WeatherApiClient(HttpClient http) => _http = http;

        public async Task<List<WeatherResult>> SearchAsync(string query, int limit, string units, CancellationToken ct = default)
        {
            var url = $"api/weather/search?q={Uri.EscapeDataString(query)}&limit={limit}&units={units}";
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new();

            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<WeatherResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
    }
}