using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WD.Models.WDModels;
using System.Globalization;

[ApiController] // Indica que esta clase es un controlador de API
[Route("api/[controller]")] // Ruta base para acceder al controlador
public class WeatherController : ControllerBase
{
    private readonly HttpClient _httpClient; // Cliente HTTP para hacer peticiones a la API externa
    private readonly string _apiKey = "5a93ba6dc0a9b1e4988c96ec7b82385d"; // Clave de acceso a OpenWeather

    public WeatherController(HttpClient httpClient)
    {
        _httpClient = httpClient; // Se inyecta HttpClient por dependencia
    }

    [HttpGet("search")] // Endpoint que se accede con GET en /api/weather/search
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10, [FromQuery] string? units = "metric")
    {
        // Normalizar las unidades, solo acepta "metric" o "imperial"
        var unitsNorm = (units?.ToLowerInvariant()) switch
        {
            "imperial" => "imperial",
            _ => "metric"
        };

        // Construir URL para obtener coordenadas segun el nombre de la ciudad
        var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={q}&limit={limit}&appid={_apiKey}";
        var geoResponse = await _httpClient.GetStringAsync(geoUrl); // Llamada HTTP
        var geoResults = JsonSerializer.Deserialize<List<GeoLocationResult>>(geoResponse) ?? new();

        var results = new List<WeatherResult>(); // Lista final de resultados con el clima

        // Recorrer cada ubicacion encontrada
        foreach (var geo in geoResults)
        {
            // Construir URL para obtener clima segun coordenadas
            var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={geo.Lat}&lon={geo.Lon}&units={unitsNorm}&lang=es&appid={_apiKey}";
            var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

            // Parsear la respuesta JSON
            using var doc = JsonDocument.Parse(weatherResponse);
            var root = doc.RootElement;

            // Obtener propiedades principales
            var weather = root.GetProperty("weather")[0];
            var main = root.GetProperty("main");

            // Funciones para obtener valores numericos de manera segura
            double? GetDouble(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDouble() : null;
            int? GetInt(JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

            // Intentar obtener el nombre del pais de manera mas legible
            string countryName;
            try { countryName = new RegionInfo(geo.Country).DisplayName; }
            catch { countryName = geo.Country; }

            // Coordenadas
            double? lat = null, lon = null;
            if (root.TryGetProperty("coord", out var coord))
            {
                lat = GetDouble(coord, "lat");
                lon = GetDouble(coord, "lon");
            }

            // Viento
            double? windSpeed = null, windGust = null;
            int? windDeg = null;
            if (root.TryGetProperty("wind", out var wind))
            {
                windSpeed = GetDouble(wind, "speed");
                windGust = GetDouble(wind, "gust");
                windDeg = GetInt(wind, "deg");
            }

            // Nubes
            int? cloudsAll = null;
            if (root.TryGetProperty("clouds", out var clouds))
            {
                cloudsAll = GetInt(clouds, "all");
            }

            // Crear objeto con toda la informacion del clima
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

            // Agregar resultado a la lista
            results.Add(item);
        }

        // Devolver la lista de resultados en formato JSON
        return Ok(results);
    }
}
