using Microsoft.AspNetCore.Http;
using WD.Mvc.Models;
using WD.Models.WDModels;
using WD.Mvc.Services;

namespace WD.Mvc.Services
{
    // Servicio para manejar la logica del dashboard (pagina principal)
    public class DashboardService
    {
        private readonly UserService _userService;           // Servicio para manejar usuarios
        private readonly FavoritosService _favoritosService; // Servicio para manejar favoritos
        private readonly WeatherApiClient _weatherApi;       // Cliente para consumir la API de clima

        // Constructor con inyeccion de dependencias
        public DashboardService(UserService userService, FavoritosService favoritosService, WeatherApiClient weatherApi)
        {
            _userService = userService;
            _favoritosService = favoritosService;
            _weatherApi = weatherApi;
        }

        // Obtener unidades de temperatura del usuario (metric o imperial)
        public Task<(string units, string symbol)> GetUnitsAsync(HttpContext http) =>
            _userService.GetTemperatureUnitsAsync(http);

        // Obtener top favoritos del usuario con informacion de clima
        public async Task<List<FavoritoClimaViewModel>> GetTopFavoritosAsync(HttpContext http, int count, CancellationToken ct = default)
        {
            // Obtener Id del usuario logueado
            var userId = _userService.GetUsuarioId(http);
            if (userId is null) return new List<FavoritoClimaViewModel>();

            // Obtener unidades de temperatura
            var (units, _) = await _userService.GetTemperatureUnitsAsync(http);

            // Obtener favoritos del usuario con clima usando el servicio
            return await _favoritosService.GetTopFavoritosAsync(userId.Value, count, units, ct);
        }

        // Buscar el clima de una ciudad usando la API externa
        public async Task<List<WeatherResult>> SearchAsync(HttpContext http, string searchQuery, int limit, CancellationToken ct = default)
        {
            // Obtener unidades de temperatura
            var (units, _) = await _userService.GetTemperatureUnitsAsync(http);

            // Ejecutar busqueda en la API de clima
            return await _weatherApi.SearchAsync(searchQuery, limit, units, ct);
        }
    }
}
