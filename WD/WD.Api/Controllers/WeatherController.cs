using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using WD.Models.WDModels;
using System.Globalization;

// Marca esta clase como un controlador de API
[ApiController]
// Define la ruta base de este controlador como "api/Weather"
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    // Cliente HTTP para realizar peticiones a servicios externos
    private readonly HttpClient _httpClient;
    // Llave de la API de OpenWeatherMap
    private readonly string _apiKey = "5a93ba6dc0a9b1e4988c96ec7b82385d";

    // Constructor que recibe una instancia de HttpClient mediante inyeccion de dependencias
    public WeatherController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Metodo HTTP GET expuesto en la ruta api/Weather/search
    [HttpGet("search")]
    // Este metodo busca informacion del clima a partir de un parametro de ciudad o localidad (q)
    // y la cantidad maxima de resultados (limit)
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
    {
        // Construye la URL para consultar la API de geolocalizacion de OpenWeatherMap
        var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={q}&limit={limit}&appid={_apiKey}";

        // Realiza la peticion HTTP para obtener coordenadas geograficas (latitud y longitud)
        var geoResponse = await _httpClient.GetStringAsync(geoUrl);

        // Deserializa la respuesta JSON en una lista de objetos GeoLocationResult
        var geoResults = JsonSerializer.Deserialize<List<GeoLocationResult>>(geoResponse);

        // Crea una lista donde se almacenaran los resultados de clima
        var results = new List<WeatherResult>();

        // Recorre cada resultado geografico devuelto por la API
        foreach (var geo in geoResults)
        {
            // Construye la URL para consultar el clima actual en base a latitud y longitud
            var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={geo.Lat}&lon={geo.Lon}&units=metric&lang=es&appid={_apiKey}";

            // Realiza la peticion HTTP para obtener datos del clima
            var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);

            // Parsea la respuesta JSON
            using var doc = JsonDocument.Parse(weatherResponse);
            var root = doc.RootElement;

            // Obtiene la seccion "weather" del JSON (es un array, por eso [0])
            var weather = root.GetProperty("weather")[0];
            // Obtiene la seccion "main" que tiene datos como temperatura y humedad
            var main = root.GetProperty("main");
            // Obtiene la seccion "sys" que incluye informacion del pais
            var sys = root.GetProperty("sys");

            // Variable para almacenar el nombre completo del pais
            string countryName;
            try
            {
                // Intenta obtener el nombre completo del pais usando el codigo ISO (ejemplo "US" -> "Estados Unidos")
                countryName = new RegionInfo(geo.Country).DisplayName;
            }
            catch
            {
                // Si ocurre un error (por ejemplo, codigo invalido), se usa el codigo del pais directamente
                countryName = geo.Country;
            }

            // Crea un objeto WeatherResult con la informacion obtenida
            results.Add(new WeatherResult
            {
                Name = geo.Name, // Nombre de la ciudad o localidad
                Country = countryName, // Nombre completo del pais
                WeatherMain = weather.GetProperty("main").GetString(), // Estado principal del clima (ej. "Clear")
                WeatherDescription = weather.GetProperty("description").GetString(), // Descripcion del clima (ej. "cielo claro")
                Temperature = main.GetProperty("temp").GetDouble(), // Temperatura en grados Celsius
                Humidity = main.GetProperty("humidity").GetInt32() // Humedad relativa en porcentaje
            });
        }

        // Devuelve la lista de resultados al cliente en formato JSON con un codigo HTTP 200 OK
        return Ok(results);
    }
}
