using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Services;
using WD.Repository.Interfaces;
using WD.Mvc.Models;

namespace WD.Mvc.Controllers
{
    [Route("Public")]
    public class PublicController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly FavoritosService _favoritosService;

        public PublicController(IUsuarioRepository usuarioRepo, FavoritosService favoritosService)
        {
            _usuarioRepo = usuarioRepo;
            _favoritosService = favoritosService;
        }

        [HttpGet("Top3/{id:int?}")]
        [HttpGet("Top3")]
        public async Task<IActionResult> Top3(int? id, CancellationToken ct = default)
        {
            if (id is null) return NotFound("Falta el id de usuario.");

            var usuario = await _usuarioRepo.GetByIdAsync(id.Value);
            if (usuario is null) return NotFound("Usuario no encontrado.");
            if (!usuario.TopFavoritosPublico) return Forbid();

            var units = (usuario.UnidadTemperatura == "F") ? "imperial" : "metric";
            var symbol = (units == "imperial") ? "°F" : "°C";
            ViewBag.TempUnitSymbol = symbol;
            ViewBag.UsuarioNombre = usuario.Nombre;

            var top3 = await _favoritosService.GetTopFavoritosAsync(usuario.IdUsuario, 3, units, ct);
            return View(top3);
        }

        // Explora Top 3 de todos los usuarios con Top público (sin concurrencia sobre DbContext)
        [HttpGet("Explore")]
        public async Task<IActionResult> Explore(CancellationToken ct = default)
        {
            var usuarios = (await _usuarioRepo.GetAllAsync())
                .Where(u => u.TopFavoritosPublico)
                .ToList();

            var model = new List<PublicUserTopViewModel>(usuarios.Count);
            foreach (var u in usuarios)
            {
                var units = (u.UnidadTemperatura == "F") ? "imperial" : "metric";
                var symbol = (units == "imperial") ? "°F" : "°C";
                var top = await _favoritosService.GetTopFavoritosAsync(u.IdUsuario, 3, units, ct);

                model.Add(new PublicUserTopViewModel
                {
                    UsuarioId = u.IdUsuario,
                    Nombre = u.Nombre,
                    Units = units,
                    UnitSymbol = symbol,
                    Top = top
                });
            }

            return View(model);
        }
    }
}