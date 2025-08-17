using Microsoft.AspNetCore.Http;
using WD.Models;
using WD.Repository.Interfaces;

namespace WD.Mvc.Services
{
    public class UserService
    {
        private readonly IUsuarioRepository _usuarioRepo;

        public UserService(IUsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public int? GetUsuarioId(HttpContext http)
        {
            var s = http.Session.GetString("UsuarioId");
            return int.TryParse(s, out var id) ? id : null;
        }

        public async Task<Usuario?> GetUsuarioActualAsync(HttpContext http)
        {
            var id = GetUsuarioId(http);
            return id.HasValue ? await _usuarioRepo.GetByIdAsync(id.Value) : null;
        }

        public async Task<(string units, string symbol)> GetTemperatureUnitsAsync(HttpContext http)
        {
            var user = await GetUsuarioActualAsync(http);
            var units = (user?.UnidadTemperatura == "F") ? "imperial" : "metric";
            var symbol = (units == "imperial") ? "°F" : "°C";
            return (units, symbol);
        }

        public void RefreshSessionNombre(HttpContext http, string nombre) =>
            http.Session.SetString("UsuarioNombre", nombre);
    }
}