using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WD.Models;
using System.Security.Claims;
using WD.Data.DB;
using System.Linq;

namespace WD.Mvc.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly WeatherDbContext _context;

        public FavoritesController(WeatherDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Obtener ID del usuario logueado (asumiendo que se guarda como int en tu tabla)
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdStr))
            {
                // No hay sesión activa, redirige a login
                return RedirectToAction("Login", "Usuarios");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            var favoritos = _context.Favoritos
                .Where(f => f.IdUsuario == usuarioId)
                .OrderByDescending(f => f.FechaAgregado)
                .ToList();

            return View(favoritos);
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
            }

            return RedirectToAction("Index");
        }
    }
}
