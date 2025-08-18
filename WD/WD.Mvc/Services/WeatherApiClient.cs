using System.Text.Json;
using WD.Models.WDModels;

namespace WD.Mvc.Services
{
    // Cliente para consumir la API de clima 
    public class WeatherApiClient
    {
        private readonly HttpClient _http;

        // HttpClient se inyecta via constructor 
        public WeatherApiClient(HttpClient http) => _http = http;

        public async Task<List<WeatherResult>> SearchAsync(string query, int limit, string units, CancellationToken ct = default)
        {
            // Construye la URL de consulta
            var url = $"api/weather/search?q={Uri.EscapeDataString(query)}&limit={limit}&units={units}";
            // Hace la llamada GET
            using var resp = await _http.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new();
            
            // Lee el JSON y lo convierte a objetos WeatherResult
            var json = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<WeatherResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
    }
}