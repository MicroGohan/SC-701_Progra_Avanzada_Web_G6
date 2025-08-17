using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WD.Models.WDModels;
using WD.Mvc.Models;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WeatherApiClient _weatherApi;
        private readonly FavoritosService _favoritosService;
        private readonly UserService _userService;

        public HomeController(ILogger<HomeController> logger, WeatherApiClient weatherApi, FavoritosService favoritosService, UserService userService)
        {
            _logger = logger;
            _weatherApi = weatherApi;
            _favoritosService = favoritosService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> IndexAsync(CancellationToken ct = default)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var (units, symbol) = await _userService.GetTemperatureUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            var top3 = await _favoritosService.GetTopFavoritosAsync(usuarioId.Value, 3, units, ct);
            ViewBag.Top3Favoritos = top3;

            return View(null);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery, CancellationToken ct = default)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            ViewBag.SearchQuery = searchQuery;

            var (units, symbol) = await _userService.GetTemperatureUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            var top3 = await _favoritosService.GetTopFavoritosAsync(usuarioId.Value, 3, units, ct);
            ViewBag.Top3Favoritos = top3;

            if (string.IsNullOrWhiteSpace(searchQuery))
                return View(null);

            var results = await _weatherApi.SearchAsync(searchQuery, 10, units, ct);
            return View(results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
