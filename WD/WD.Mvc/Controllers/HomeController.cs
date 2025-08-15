using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WD.Models.WDModels;
using WD.Mvc.Models;
using WD.Mvc.Services;
using WD.Data.DB;

namespace WD.Mvc.Controllers
{
    // Define un controlador MVC llamado HomeController
    public class HomeController : Controller
    {
        // Logger para registrar mensajes e informacion en el sistema
        private readonly ILogger<HomeController> _logger;

        // Fabrica para crear instancias de HttpClient
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly FavoritosService _favoritosService;
        private readonly WeatherDbContext _context;


        // Constructor del controlador que recibe inyeccion de dependencias
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, FavoritosService favoritosService, WeatherDbContext context)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _favoritosService = favoritosService;
            _context = context;
        }

        // Metodo que responde a peticiones HTTP GET para la vista principal (Index)
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            // Verifica si no existe un usuario logueado en la sesion
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                // Si no hay sesion, redirige al login
                return RedirectToAction("Login", "Usuarios");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId")!);

            // Establecer el símbolo de unidad para la vista (°C o °F)
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            ViewBag.TempUnitSymbol = (user?.UnidadTemperatura == "F") ? "°F" : "°C";

            // Llama al método del FavoritesController (o mejor, mueve la lógica a un servicio compartido)
            var top5 = await _favoritosService.GetTop5FavoritosAsync(usuarioId);

            ViewBag.Top5Favoritos = top5;

            // Devuelve la vista Index sin datos (null)
            return View(null);
        }

        // Metodo que responde a peticiones HTTP POST para la vista Index
        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery)
        {
            // Verifica si el usuario esta logueado
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Usuarios");
            }

            // Guarda el valor buscado en la ViewBag para poder mostrarlo en la vista
            ViewBag.SearchQuery = searchQuery;

            // Si no se ingreso texto para buscar, devuelve la vista vacia
            if (string.IsNullOrWhiteSpace(searchQuery))
                return View(null);

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId")!);
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            var units = (user?.UnidadTemperatura == "F") ? "imperial" : "metric"; // F -> imperial, C -> metric
            ViewBag.TempUnitSymbol = (units == "imperial") ? "°F" : "°C";

            // Crea un cliente HTTP usando la fabrica inyectada
            var client = _httpClientFactory.CreateClient();

            // Construye la URL para llamar a la API local de Weather
            var url = $"https://localhost:7215/api/weather/search?q={Uri.EscapeDataString(searchQuery)}&limit=10&units={units}";

            // Realiza la llamada HTTP a la API
            var response = await client.GetAsync(url);

            // Verifica si la respuesta fue exitosa
            if (!response.IsSuccessStatusCode)
            {
                // Si hay error en la respuesta, agrega un error al ModelState
                ModelState.AddModelError("", "Error al consultar el clima.");
                return View(null);
            }

            // Lee el contenido JSON devuelto por la API
            var json = await response.Content.ReadAsStringAsync();

            // Deserializa el JSON en una lista de objetos WeatherResult
            var results = JsonSerializer.Deserialize<List<WeatherResult>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Devuelve la vista with los resultados de la busqueda
            return View(results);
        }

        // Metodo para mostrar la vista de error, usado cuando ocurre alguna excepcion o error en el sistema
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Devuelve la vista Error con informacion del RequestId para poder rastrear errores
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
