using Microsoft.AspNetCore.Http;
using WD.Mvc.Models;

namespace WD.Mvc.Services
{
    // Servicio que maneja la logica de configuracion y ajustes de usuario
    public class SettingsAppService
    {
        private readonly UserService _userService; // Servicio de usuario para operaciones sobre la cuenta

        public SettingsAppService(UserService userService)
        {
            _userService = userService;
        }

        // Obtener los datos del usuario para la pagina de configuracion
        public async Task<(bool authorized, UserSettingsViewModel vm, List<string> continentes)> GetIndexAsync(HttpContext http)
        {
            var user = await _userService.GetUsuarioActualAsync(http);
            if (user == null) return (false, new UserSettingsViewModel(), new List<string>());

            // Crear ViewModel con los datos del usuario
            var vm = new UserSettingsViewModel
            {
                Nombre = user.Nombre,
                Email = user.Email,
                Continente = user.Continente,
                UnidadTemperatura = string.IsNullOrWhiteSpace(user.UnidadTemperatura) ? "C" : user.UnidadTemperatura,
                TopFavoritosPublico = user.TopFavoritosPublico
            };

            // Lista fija de continentes para mostrar en el formulario
            var continentes = new List<string> { "Africa", "America", "Asia", "Europa", "Oceania" };
            return (true, vm, continentes);
        }

        // Actualizar perfil del usuario
        public Task<(bool ok, string? error)> UpdateProfileAsync(HttpContext http, UserSettingsViewModel model) =>
            _userService.UpdateProfileAsync(http, model);

        // Cambiar la contraseña del usuario
        public Task<(bool ok, string? error)> ChangePasswordAsync(HttpContext http, string? actual, string? nueva, string? confirmacion) =>
            _userService.ChangePasswordAsync(http, actual, nueva, confirmacion);

        // Actualizar visibilidad del Top 3 publico
        public async Task<(bool ok, string? error, string message)> UpdateTopPublicoAsync(HttpContext http, bool topFavoritosPublico)
        {
            var (ok, error) = await _userService.UpdateTopPublicoAsync(http, topFavoritosPublico);
            // Crear mensaje amigable segun la nueva configuracion
            var message = topFavoritosPublico
                ? "Tu Top 3 ahora es publico."
                : "Tu Top 3 ahora es privado.";
            return (ok, error, message);
        }
    }
}
