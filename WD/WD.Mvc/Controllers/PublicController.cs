using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    // Controlador para exponer informacion publica (Top favoritos)
    [Route("Public")]
    public class PublicController : Controller
    {
        private readonly PublicTopService _publicTopService;

        // Constructor con inyeccion de dependencia del servicio
        public PublicController(PublicTopService publicTopService)
        {
            _publicTopService = publicTopService;
        }

        // Endpoint GET: /Public/Top3/{id}
        // Muestra el Top 3 de favoritos de un usuario especifico
        [HttpGet("Top3/{id:int?}")]
        [HttpGet("Top3")]
        public async Task<IActionResult> Top3(int? id, CancellationToken ct = default)
        {
            // Si no se recibe el id se devuelve error 404
            if (id is null) return NotFound("Falta el id de usuario.");

            // Obtener el top 3 desde el servicio
            var result = await _publicTopService.GetTop3Async(id.Value, ct);

            // Validaciones segun el resultado
            if (!result.ok)
            {
                if (result.forbidden) return Forbid(); // No tiene permiso
                return NotFound(result.error ?? "Usuario no encontrado.");
            }

            // Pasar datos extra a la vista
            ViewBag.TempUnitSymbol = result.symbol;       // Simbolo de la unidad de temperatura
            ViewBag.UsuarioNombre = result.usuarioNombre; // Nombre del usuario dueño del top

            // Renderizar vista con la lista top
            return View(result.top);
        }

        // Endpoint GET: /Public/Explore
        // Permite explorar el Top 3 de todos los usuarios que tienen su Top configurado como publico
        // Nota: se hace sin concurrencia sobre DbContext para evitar problemas de acceso
        [HttpGet("Explore")]
        public async Task<IActionResult> Explore(CancellationToken ct = default)
        {
            var model = await _publicTopService.ExploreAsync(ct);
            return View(model);
        }
    }
}
