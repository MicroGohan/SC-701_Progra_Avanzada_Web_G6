using Microsoft.AspNetCore.Http;
using WD.Mvc.Models;
using WD.Repository.Interfaces;
using WD.Models;

namespace WD.Mvc.Services
{
    // Servicio que maneja la logica de favoritos del usuario
    public class FavoritosService
    {
        private readonly IFavoritoRepository _favoritoRepo; // Repositorio para acceder a la BD de favoritos
        private readonly WeatherApiClient _weatherApi;       // Cliente para consumir la API de clima
        private readonly UserService _userService;          // Servicio de usuarios

        // Constructor con inyeccion de dependencias
        public FavoritosService(IFavoritoRepository favoritoRepo, WeatherApiClient weatherApi, UserService userService)
        {
            _favoritoRepo = favoritoRepo;
            _weatherApi = weatherApi;
            _userService = userService;
        }

        // Obtener los favoritos del usuario con datos de clima
        public async Task<(bool authorized, string symbol, List<FavoritoClimaViewModel> model)>
            GetIndexAsync(HttpContext http, CancellationToken ct = default)
        {
            var userId = _userService.GetUsuarioId(http);
            if (userId is null) return (false, "", new List<FavoritoClimaViewModel>());

            var (units, symbol) = await _userService.GetTemperatureUnitsAsync(http);
            var modelo = await GetFavoritosConClimaAsync(userId.Value, units, ct);
            return (true, symbol, modelo);
        }

        // Agregar un favorito
        public async Task<(bool authorized, bool agregado, int? favoritoId, string messageAjax, string tempHtmlMessage, int? ultimoFavoritoId, string searchQuery)>
            AddAsync(HttpContext http, string ciudad, string pais, CancellationToken ct = default)
        {
            var userId = _userService.GetUsuarioId(http);
            if (userId is null)
            {
                return (false, false, null, "No autorizado", "", null, "");
            }

            var (agregado, favorito) = await TryAddFavoritoAsync(userId.Value, ciudad, pais, ct);

            // Mensajes para mostrar en ajax o en TempData
            var messageAjax = agregado
                ? $"✅ Ciudad {ciudad} ({pais}) agregada a favoritos."
                : "⚠️ La ciudad ya esta en tus favoritos.";

            var tempHtml = agregado
                ? $"✅ Ciudad <strong>{ciudad}</strong> del Pais <strong>{pais}</strong> ha sido agregada a favoritos correctamente."
                : "⚠️ La ciudad ya esta en tus favoritos.";

            return (true, agregado, favorito?.IdFavorito, messageAjax, tempHtml, favorito?.IdFavorito, $"{ciudad}, {pais}");
        }

        // Eliminar un favorito
        public async Task<(bool authorized, bool ok, string eliminadoMensaje)>
            DeleteAsync(HttpContext http, int idFavorito, CancellationToken ct = default)
        {
            var userId = _userService.GetUsuarioId(http);
            if (userId is null) return (false, false, "");

            var ok = await TryDeleteAsync(userId.Value, idFavorito, ct);
            var msg = ok
                ? "✅ El favorito ha sido eliminado correctamente."
                : "⚠️ Favorito no encontrado.";
            return (true, ok, msg);
        }

        // Actualizar prioridad de un favorito
        public async Task<(bool authorized, bool ok, string? mensaje)>
            UpdatePrioridadAsync(HttpContext http, int idFavorito, string prioridad, CancellationToken ct = default)
        {
            var userId = _userService.GetUsuarioId(http);
            if (userId is null) return (false, false, null);

            var ok = await TryUpdatePrioridadAsync(userId.Value, idFavorito, prioridad, ct);
            var mensaje = ok ? $"✅ Prioridad actualizada a <strong>{prioridad}</strong>." : null;
            return (true, ok, mensaje);
        }

        // Actualizar descripcion de un favorito
        public async Task<(bool authorized, bool ok, string? descripcionGuardada, string? favoritoMensaje)>
            UpdateDescripcionAsync(HttpContext http, int idFavorito, string descripcion, CancellationToken ct = default)
        {
            var userId = _userService.GetUsuarioId(http);
            if (userId is null) return (false, false, null, null);

            var ok = await TryUpdateDescripcionAsync(userId.Value, idFavorito, descripcion, ct);
            var guardada = ok ? "✅ Descripcion guardada con exito." : null;
            var favMsg = ok ? null : "⚠️ No se pudo agregar la descripcion.";
            return (true, ok, guardada, favMsg);
        }

        // Obtener top 5 favoritos de un usuario
        public async Task<List<FavoritoClimaViewModel>> GetTop5FavoritosAsync(int usuarioId, string units = "metric", CancellationToken ct = default)
        {
            var favoritos = await _favoritoRepo.GetTop5ByUserAsync(usuarioId, ct);
            return await EnriquecerConClimaAsync(favoritos, units, ct);
        }

        // Obtener top N favoritos de un usuario
        public async Task<List<FavoritoClimaViewModel>> GetTopFavoritosAsync(int usuarioId, int count, string units = "metric", CancellationToken ct = default)
        {
            var favoritosOrdenados = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            var top = favoritosOrdenados.Take(count);
            return await EnriquecerConClimaAsync(top, units, ct);
        }

        // Obtener todos los favoritos con clima
        public async Task<List<FavoritoClimaViewModel>> GetFavoritosConClimaAsync(int usuarioId, string units, CancellationToken ct = default)
        {
            var favoritos = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            return await EnriquecerConClimaAsync(favoritos, units, ct);
        }

        // Intentar agregar un favorito
        public async Task<(bool agregado, Favorito? favorito)> TryAddFavoritoAsync(int usuarioId, string ciudad, string pais, CancellationToken ct = default)
        {
            var existentes = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            if (existentes.Any(f => f.Ciudad == ciudad && f.Pais == pais))
                return (false, null);

            var favorito = new Favorito
            {
                IdUsuario = usuarioId,
                Ciudad = ciudad,
                Pais = pais,
                FechaAgregado = DateOnly.FromDateTime(DateTime.Now)
            };
            await _favoritoRepo.AddAsync(favorito, ct);
            return (true, favorito);
        }

        // Intentar eliminar un favorito
        public async Task<bool> TryDeleteAsync(int usuarioId, int idFavorito, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            await _favoritoRepo.DeleteAsync(fav, ct);
            return true;
        }

        // Intentar actualizar prioridad de un favorito
        public async Task<bool> TryUpdatePrioridadAsync(int usuarioId, int idFavorito, string prioridad, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            fav.Prioridad = prioridad;
            await _favoritoRepo.UpdateAsync(fav, ct);
            return true;
        }

        // Intentar actualizar descripcion de un favorito
        public async Task<bool> TryUpdateDescripcionAsync(int usuarioId, int idFavorito, string descripcion, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            fav.Descripcion = descripcion;
            await _favoritoRepo.UpdateAsync(fav, ct);
            return true;
        }

        // Enriquecer favoritos con datos de clima desde la API externa
        private async Task<List<FavoritoClimaViewModel>> EnriquecerConClimaAsync(IEnumerable<Favorito> favoritos, string units, CancellationToken ct)
        {
            var salida = new List<FavoritoClimaViewModel>();
            foreach (var fav in favoritos)
            {
                // Buscar clima de la ciudad favorita
                var results = await _weatherApi.SearchAsync($"{fav.Ciudad},{fav.Pais}", 1, units, ct);
                var clima = results.FirstOrDefault();

                // Crear ViewModel combinando favorito + clima
                salida.Add(new FavoritoClimaViewModel
                {
                    Favorito = fav,
                    WeatherDescription = clima?.WeatherDescription,
                    Temperatura = clima?.Temperature,
                    Humedad = clima?.Humidity
                });
            }
            return salida;
        }
    }
}
