using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Models;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly FavoritosService _favoritosService;
        private readonly UserService _userService;

        public FavoritesController(FavoritosService favoritosService, UserService userService)
        {
            _favoritosService = favoritosService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var (units, symbol) = await _userService.GetTemperatureUnitsAsync(HttpContext);
            ViewBag.TempUnitSymbol = symbol;

            var modelo = await _favoritosService.GetFavoritosConClimaAsync(usuarioId.Value, units, ct);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string ciudad, string pais, CancellationToken ct)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var (agregado, favorito) = await _favoritosService.TryAddFavoritoAsync(usuarioId.Value, ciudad, pais, ct);
            if (agregado)
            {
                TempData["FavoritoMensaje"] = $"✅ Ciudad <strong>{ciudad}</strong> del País <strong>{pais}</strong> ha sido agregada a favoritos correctamente.";
                TempData["UltimoFavoritoId"] = favorito!.IdFavorito;
            }
            else
            {
                TempData["FavoritoMensaje"] = "⚠️ La ciudad ya está en tus favoritos.";
            }

            return RedirectToAction("Index", "Home", new { searchQuery = $"{ciudad}, {pais}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int idFavorito, CancellationToken ct)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var ok = await _favoritosService.TryDeleteAsync(usuarioId.Value, idFavorito, ct);
            TempData["EliminadoMensaje"] = ok
                ? "✅ El favorito ha sido eliminado correctamente."
                : "⚠️ Favorito no encontrado.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrioridad(int idFavorito, string prioridad, CancellationToken ct)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var ok = await _favoritosService.TryUpdatePrioridadAsync(usuarioId.Value, idFavorito, prioridad, ct);
            if (ok)
            {
                TempData["Mensaje"] = $"✅ Prioridad actualizada a <strong>{prioridad}</strong>.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescripcion(int idFavorito, string descripcion, CancellationToken ct)
        {
            var usuarioId = _userService.GetUsuarioId(HttpContext);
            if (usuarioId is null) return RedirectToAction("Login", "Usuarios");

            var ok = await _favoritosService.TryUpdateDescripcionAsync(usuarioId.Value, idFavorito, descripcion, ct);
            TempData["DescripcionGuardada"] = ok
                ? "✅ Descripción guardada con éxito."
                : null;
            if (!ok) TempData["FavoritoMensaje"] = "⚠️ No se pudo agregar la descripción.";

            return RedirectToAction("Index", "Home");
        }
    }
}
