using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    // Controlador para manejar favoritos
    public class FavoritesController : Controller
    {
        private readonly FavoritosService _favoritosService;

        // Constructor con inyeccion de dependencia del servicio de favoritos
        public FavoritesController(FavoritosService favoritosService)
        {
            _favoritosService = favoritosService;
        }

        // Vista principal de favoritos
        public async Task<IActionResult> Index(CancellationToken ct = default)
        {
            // Llamada al servicio para obtener informacion inicial
            var (authorized, symbol, model) = await _favoritosService.GetIndexAsync(HttpContext, ct);

            // Si no esta autorizado redirige al login
            if (!authorized) return RedirectToAction("Login", "Usuarios");

            // Pasar simbolo de la unidad de temperatura a la vista
            ViewBag.TempUnitSymbol = symbol;

            // Enviar el modelo a la vista
            return View(model);
        }

        // Agregar un nuevo favorito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string ciudad, string pais, CancellationToken ct)
        {
            // Llamar al servicio para agregar el favorito
            var (authorized, agregado, favoritoId, messageAjax, tempHtmlMessage, ultimoFavoritoId, searchQuery)
                = await _favoritosService.AddAsync(HttpContext, ciudad, pais, ct);

            // Verificar autorizacion
            if (!authorized)
            {
                if (IsAjaxRequest(Request))
                    return Unauthorized(new { ok = false, message = "No autorizado" });
                return RedirectToAction("Login", "Usuarios");
            }

            // Respuesta si la peticion fue Ajax
            if (IsAjaxRequest(Request))
            {
                return Json(new
                {
                    ok = agregado,
                    message = messageAjax,
                    favoritoId = agregado ? favoritoId : (int?)null
                });
            }

            // Guardar mensajes en TempData para mostrar en la vista
            if (agregado)
            {
                TempData["FavoritoMensaje"] = tempHtmlMessage;
                TempData["UltimoFavoritoId"] = ultimoFavoritoId;
            }
            else
            {
                TempData["FavoritoMensaje"] = tempHtmlMessage;
            }

            // Redirigir a la pagina principal con la busqueda
            return RedirectToAction("Index", "Home", new { searchQuery });
        }

        // Eliminar un favorito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int idFavorito, CancellationToken ct)
        {
            var (authorized, _, eliminadoMensaje) = await _favoritosService.DeleteAsync(HttpContext, idFavorito, ct);
            if (!authorized) return RedirectToAction("Login", "Usuarios");

            TempData["EliminadoMensaje"] = eliminadoMensaje;
            return RedirectToAction("Index");
        }

        // Actualizar prioridad de un favorito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrioridad(int idFavorito, string prioridad, CancellationToken ct)
        {
            var (authorized, ok, mensaje) = await _favoritosService.UpdatePrioridadAsync(HttpContext, idFavorito, prioridad, ct);
            if (!authorized) return RedirectToAction("Login", "Usuarios");

            if (ok && mensaje is not null)
            {
                TempData["Mensaje"] = mensaje;
            }

            return RedirectToAction("Index");
        }

        // Actualizar descripcion de un favorito
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDescripcion(int idFavorito, string descripcion, CancellationToken ct)
        {
            var (authorized, ok, descripcionGuardada, favoritoMensaje) = await _favoritosService.UpdateDescripcionAsync(HttpContext, idFavorito, descripcion, ct);
            if (!authorized) return RedirectToAction("Login", "Usuarios");

            TempData["DescripcionGuardada"] = ok ? descripcionGuardada : null;
            if (!ok) TempData["FavoritoMensaje"] = favoritoMensaje;

            return RedirectToAction("Index", "Home");
        }

        // Metodo auxiliar para detectar si la peticion es Ajax
        private static bool IsAjaxRequest(HttpRequest request)
        {
            var xrw = request.Headers["X-Requested-With"].ToString();
            var accept = request.Headers["Accept"].ToString();
            return string.Equals(xrw, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                   || accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
