using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WD.Models.WDModels;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "5a93ba6dc0a9b1e4988c96ec7b82385d";

    public WeatherController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10, [FromQuery] string? units = "metric")
    {
        var unitsNorm = (units?.ToLowerInvariant()) switch
        {
            "imperial" => "imperial",
            _ => "metric"
        };

        var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={q}&limit={limit}&appid={_apiKey}";
        var geoResponse = await _httpClient.GetStringAsync(geoUrl);
        var geoResults = JsonSerializer.Deserialize<List<GeoLocationResult>>(geoResponse) ?? new();

        var results = new List<WeatherResult>();

        foreach (var geo in geoResults)
        {
            var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={geo.Lat}&lon={geo.Lon}&units={unitsNorm}&lang=es&appid={_apiKey}";
            var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

            using var doc = JsonDocument.Parse(weatherResponse);
            var root = doc.RootElement;

            var weather = root.GetProperty("weather")[0];
            var main = root.GetProperty("main");
 
            double? GetDouble(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;
            int? GetInt(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

            string countryName;
            try { countryName = new RegionInfo(geo.Country).DisplayName; }
            catch { countryName = geo.Country; }

            // coord
            double? lat = null, lon = null;
            if (root.TryGetProperty("coord", out var coord))
            {
                lat = GetDouble(coord, "lat");
                lon = GetDouble(coord, "lon");
            }

            // viento
            double? windSpeed = null, windGust = null;
            int? windDeg = null;
            if (root.TryGetProperty("wind", out var wind))
            {
                windSpeed = GetDouble(wind, "speed");
                windGust = GetDouble(wind, "gust");
                windDeg = GetInt(wind, "deg");
            }

            // nubes
            int? cloudsAll = null;
            if (root.TryGetProperty("clouds", out var clouds))
            {
                cloudsAll = GetInt(clouds, "all");
            }

            var item = new WeatherResult
            {
                Name = geo.Name,
                Country = countryName,
                WeatherMain = weather.GetProperty("main").GetString(),
                WeatherDescription = weather.GetProperty("description").GetString(),

                Temperature = GetDouble(main, "temp"),
                Humidity = GetInt(main, "humidity"),
                Pressure = GetInt(main, "pressure"),
                SeaLevel = GetInt(main, "sea_level"),
                GroundLevel = GetInt(main, "grnd_level"),
                FeelsLike = GetDouble(main, "feels_like"),
                TempMin = GetDouble(main, "temp_min"),
                TempMax = GetDouble(main, "temp_max"),

                Lat = lat ?? geo.Lat,
                Lon = lon ?? geo.Lon,
                Visibility = GetInt(root, "visibility"),
                Cloudiness = cloudsAll,
                WindSpeed = windSpeed,
                WindDeg = windDeg,
                WindGust = windGust
            };

            results.Add(item);
        }

        return Ok(results);
    }
}
