using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WD.Models;
using System.Security.Claims;
using WD.Data.DB;
using System.Linq;
using WD.Mvc.Models;

namespace WD.Mvc.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly WeatherDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public FavoritesController(WeatherDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);

            var favoritos = _context.Favoritos
                .Where(f => f.IdUsuario == usuarioId)
                .OrderBy(f => f.Prioridad == "bajo" ? 3 :
                              f.Prioridad == "medio" ? 2 :
                              f.Prioridad == "alto" ? 1 : 0)
                .ThenByDescending(f => f.FechaAgregado)
                .ToList();

            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            var units = (user?.UnidadTemperatura == "F") ? "imperial" : "metric";
            ViewBag.TempUnitSymbol = (units == "imperial") ? "°F" : "°C";

            var favoritosClima = new List<FavoritoClimaViewModel>();
            var client = _httpClientFactory.CreateClient();

            foreach (var fav in favoritos)
            {
                // Llama a tu propia API para obtener el clima de la ciudad y país del favorito
                var url = $"https://localhost:7215/api/weather/search?q={Uri.EscapeDataString(fav.Ciudad + "," + fav.Pais)}&limit=1&units={units}";
                var response = await client.GetAsync(url);

                string? weatherDescription = null;
                double? temperatura = null;
                int? humedad = null;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var results = System.Text.Json.JsonSerializer.Deserialize<List<WD.Models.WDModels.WeatherResult>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var clima = results?.FirstOrDefault();
                    if (clima != null)
                    {
                        weatherDescription = clima.WeatherDescription;
                        temperatura = clima.Temperature;
                        humedad = clima.Humidity;
                    }
                }

                favoritosClima.Add(new FavoritoClimaViewModel
                {
                    Favorito = fav,
                    WeatherDescription = weatherDescription,
                    Temperatura = temperatura,
                    Humedad = humedad
                });
            }

            return View(favoritosClima);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(string ciudad, string pais)
        {
            // Obtener el ID del usuario desde la sesión
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            // Validación: si no hay sesión, redirige a login
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);

            // Verificar si ya existe el favorito
            bool existe = _context.Favoritos
                .Any(f => f.IdUsuario == usuarioId && f.Ciudad == ciudad && f.Pais == pais);

            if (!existe)
            {
                var favorito = new Favorito
                {
                    IdUsuario = usuarioId,
                    Ciudad = ciudad,
                    Pais = pais,
                    FechaAgregado = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Favoritos.Add(favorito);
                _context.SaveChanges();
                TempData["FavoritoMensaje"] = $"✅ Ciudad <strong>{ciudad}</strong> del País <strong>{pais}</strong> ha sido agregada a favoritos correctamente.";
                TempData["UltimoFavoritoId"] = favorito.IdFavorito;
            }
            else
            {
                TempData["FavoritoMensaje"] = "⚠️ La ciudad ya está en tus favoritos.";
            }

            return RedirectToAction("Index", "Home", new { searchQuery = $"{ciudad}, {pais}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int idFavorito)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);

            var favorito = _context.Favoritos
                .FirstOrDefault(f => f.IdFavorito == idFavorito && f.IdUsuario == usuarioId);

            if (favorito != null)
            {
                _context.Favoritos.Remove(favorito);
                _context.SaveChanges();

                TempData["EliminadoMensaje"] = $"✅ La Ciudad <strong>{favorito.Ciudad}</strong> del Pais <strong>{favorito.Pais}</strong> ha sido eliminada de favoritos correctamente.";
            }
            else
            {
                TempData["EliminadoMensaje"] = "⚠️ Favorito no encontrado.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePrioridad(int idFavorito, string prioridad)
        {
            var favorito = _context.Favoritos.FirstOrDefault(f => f.IdFavorito == idFavorito);
            if (favorito != null)
            {
                favorito.Prioridad = prioridad;
                _context.SaveChanges();
                TempData["Mensaje"] = $"✅ Prioridad actualizada a <strong>{prioridad}</strong>.";
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDescripcion(int idFavorito, string descripcion)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);

            var favorito = _context.Favoritos
                .FirstOrDefault(f => f.IdFavorito == idFavorito && f.IdUsuario == usuarioId);

            if (favorito != null)
            {
                favorito.Descripcion = descripcion;
                _context.SaveChanges();
                TempData["DescripcionGuardada"] = "✅ Descripción guardada con éxito.";
            }
            else
            {
                TempData["FavoritoMensaje"] = "⚠️ No se pudo agregar la descripción.";
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<List<FavoritoClimaViewModel>> GetTop5FavoritosAsync(int usuarioId)
        {
            var favoritos = _context.Favoritos
                .Where(f => f.IdUsuario == usuarioId)
                .OrderBy(f => f.Prioridad == "alto" ? 0 : f.Prioridad == "medio" ? 1 : 2)
                .ThenByDescending(f => f.FechaAgregado)
                .Take(5)
                .ToList();

            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            var units = (user?.UnidadTemperatura == "F") ? "imperial" : "metric";
            ViewBag.TempUnitSymbol = (units == "imperial") ? "°F" : "°C";

            var favoritosClima = new List<FavoritoClimaViewModel>();
            var client = _httpClientFactory.CreateClient();

            foreach (var fav in favoritos)
            {
                var url = $"https://localhost:7215/api/weather/search?q={Uri.EscapeDataString(fav.Ciudad + "," + fav.Pais)}&limit=1&units={units}";
                var response = await client.GetAsync(url);

                string? weatherDescription = null;
                double? temperatura = null;
                int? humedad = null;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var results = System.Text.Json.JsonSerializer.Deserialize<List<WD.Models.WDModels.WeatherResult>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var clima = results?.FirstOrDefault();
                    if (clima != null)
                    {
                        weatherDescription = clima.WeatherDescription;
                        temperatura = clima.Temperature;
                        humedad = clima.Humidity;
                    }
                }

                favoritosClima.Add(new FavoritoClimaViewModel
                {
                    Favorito = fav,
                    WeatherDescription = weatherDescription,
                    Temperatura = temperatura,
                    Humedad = humedad
                });
            }

            return favoritosClima;
        }
    }
}
