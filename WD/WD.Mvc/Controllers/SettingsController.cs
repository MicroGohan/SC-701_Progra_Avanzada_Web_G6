using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Models;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers
{
    // Controlador para manejar configuraciones de usuario
    public class SettingsController : Controller
    {
        private readonly SettingsAppService _settingsApp;

        // Constructor con inyeccion de dependencia del servicio
        public SettingsController(SettingsAppService settingsApp)
        {
            _settingsApp = settingsApp;
        }

        // Vista principal de configuraciones
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtener configuraciones iniciales del usuario
            var (authorized, vm, continentes) = await _settingsApp.GetIndexAsync(HttpContext);

            // Si no esta autorizado redirige a login
            if (!authorized) return RedirectToAction("Login", "Usuarios");

            // Guardar continentes en ViewBag para la vista
            ViewBag.Continentes = continentes;

            // Renderizar vista con el modelo de configuraciones
            return View(vm);
        }

        // Actualizar perfil del usuario (ej: nombre, correo, continente, etc.)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserSettingsViewModel model)
        {
            var (ok, error) = await _settingsApp.UpdateProfileAsync(HttpContext, model);

            // Si falla mostrar mensaje de error y recargar la vista
            if (!ok)
            {
                TempData["SettingsError"] = error ?? "No se pudo actualizar el perfil.";
                ViewBag.Continentes = (await _settingsApp.GetIndexAsync(HttpContext)).continentes;
                return View("Index", model);
            }

            // Si fue exitoso mostrar mensaje de confirmacion
            TempData["SettingsMessage"] = "Preferencias actualizadas correctamente.";
            return RedirectToAction("Index");
        }

        // Cambiar la contraseña del usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserSettingsViewModel model)
        {
            var (ok, error) = await _settingsApp.ChangePasswordAsync(
                HttpContext,
                model.PasswordActual,
                model.PasswordNuevo,
                model.PasswordConfirmacion
            );

            if (!ok)
            {
                TempData["SettingsError"] = error ?? "No se pudo actualizar la contraseña.";
                return RedirectToAction("Index");
            }

            TempData["SettingsMessage"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // Cambiar si el Top de favoritos del usuario es publico o privado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTopPublico(bool TopFavoritosPublico)
        {
            var (ok, error, message) = await _settingsApp.UpdateTopPublicoAsync(HttpContext, TopFavoritosPublico);

            if (!ok)
            {
                TempData["SettingsError"] = error ?? "No se pudo actualizar la visibilidad.";
                return RedirectToAction("Index");
            }

            TempData["SettingsMessage"] = message;
            return RedirectToAction("Index");
        }
    }
}
