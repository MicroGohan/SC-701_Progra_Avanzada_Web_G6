using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WD.Models.WDModels;
using WD.Mvc.Models;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    // Controlador principal que maneja la pagina de inicio y el dashboard
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;          // Logger para registrar eventos y errores
        private readonly DashboardService _dashboardService;       // Servicio para logica del dashboard
        private readonly UserService _userService;                 // Servicio para manejar usuarios

        // Constructor con inyeccion de dependencias
        public HomeController(ILogger<HomeController> logger, DashboardService dashboardService, UserService userService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _userService = userService;
        }

        // Metodo GET para mostrar la pagina principal
        [HttpGet]
        public async Task<IActionResult> IndexAsync(CancellationToken ct = default)
        {
            // Verificar si el usuario esta logueado
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            // Obtener unidades de temperatura (ejemplo: °C o °F)
            var (_, symbol) = await _dashboardService.GetUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            // Obtener los 3 favoritos mas recientes o destacados
            var top3 = await _dashboardService.GetTopFavoritosAsync(HttpContext, 3, ct);
            ViewBag.Top3Favoritos = top3;

            // Cargar la vista inicial sin resultados de busqueda
            return View(null);
        }

        // Metodo POST para cuando el usuario hace una busqueda
        [HttpPost]
        public async Task<IActionResult> Index(string searchQuery, CancellationToken ct = default)
        {
            // Validar usuario logueado
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            // Guardar la consulta de busqueda en la vista
            ViewBag.SearchQuery = searchQuery;

            // Obtener unidades de temperatura
            var (_, symbol) = await _dashboardService.GetUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            // Obtener top 3 favoritos para mostrar en el dashboard
            var top3 = await _dashboardService.GetTopFavoritosAsync(HttpContext, 3, ct);
            ViewBag.Top3Favoritos = top3;

            // Si no se ingreso busqueda, devolver vista vacia
            if (string.IsNullOrWhiteSpace(searchQuery))
                return View(null);

            // Ejecutar busqueda y devolver resultados
            var results = await _dashboardService.SearchAsync(HttpContext, searchQuery, 10, ct);
            return View(results);
        }

        // Metodo GET que devuelve solo la vista parcial de favoritos
        [HttpGet]
        public async Task<IActionResult> TopFavoritosPartial(int count = 3, CancellationToken ct = default)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return Unauthorized();

            // Obtener unidades de temperatura
            var (_, symbol) = await _dashboardService.GetUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            // Obtener el top de favoritos segun la cantidad indicada
            var top = await _dashboardService.GetTopFavoritosAsync(HttpContext, count, ct);

            // Devolver una vista parcial con los datos
            return PartialView("_TopFavoritos", top);
        }

        // Vista de error generica
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
