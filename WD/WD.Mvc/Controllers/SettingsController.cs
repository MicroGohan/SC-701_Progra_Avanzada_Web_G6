using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Models;
using WD.Mvc.Services;
using WD.Repository.Interfaces;

namespace WD.Mvc.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly UserService _userService;

        public SettingsController(IUsuarioRepository usuarioRepo, UserService userService)
        {
            _usuarioRepo = usuarioRepo;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetUsuarioActualAsync(HttpContext);
            if (user == null) return RedirectToAction("Login", "Usuarios");

            var vm = new UserSettingsViewModel
            {
                Nombre = user.Nombre,
                Email = user.Email,
                Continente = user.Continente,
                UnidadTemperatura = string.IsNullOrWhiteSpace(user.UnidadTemperatura) ? "C" : user.UnidadTemperatura,
                // NUEVO
                TopFavoritosPublico = user.TopFavoritosPublico
            };

            ViewBag.Continentes = new List<string> { "Africa", "America", "Asia", "Europa", "Oceania" };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserSettingsViewModel model)
        {
            var user = await _userService.GetUsuarioActualAsync(HttpContext);
            if (user == null) return RedirectToAction("Login", "Usuarios");

            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                TempData["SettingsError"] = "El nombre es requerido.";
                ViewBag.Continentes = new List<string> { "Africa", "America", "Asia", "Europa", "Oceania" };
                return View("Index", model);
            }

            user.Nombre = model.Nombre.Trim();
            user.Continente = string.IsNullOrWhiteSpace(model.Continente) ? null : model.Continente.Trim();
            user.UnidadTemperatura = (model.UnidadTemperatura == "F") ? "F" : "C";

            await _usuarioRepo.UpdateAsync(user);

            _userService.RefreshSessionNombre(HttpContext, user.Nombre);
            TempData["SettingsMessage"] = "Preferencias actualizadas correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(UserSettingsViewModel model)
        {
            var user = await _userService.GetUsuarioActualAsync(HttpContext);
            if (user == null) return RedirectToAction("Login", "Usuarios");

            if (string.IsNullOrWhiteSpace(model.PasswordActual) ||
                string.IsNullOrWhiteSpace(model.PasswordNuevo) ||
                string.IsNullOrWhiteSpace(model.PasswordConfirmacion))
            {
                TempData["SettingsError"] = "Complete todos los campos de contraseña.";
                return RedirectToAction("Index");
            }

            if (!string.Equals(user.Password, model.PasswordActual))
            {
                TempData["SettingsError"] = "La contraseña actual es incorrecta.";
                return RedirectToAction("Index");
            }

            if (!string.Equals(model.PasswordNuevo, model.PasswordConfirmacion))
            {
                TempData["SettingsError"] = "La confirmación no coincide.";
                return RedirectToAction("Index");
            }

            user.Password = model.PasswordNuevo;
            await _usuarioRepo.UpdateAsync(user);

            TempData["SettingsMessage"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index");
        }

        // NUEVO: cambiar visibilidad del Top 3 sin afectar el resto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTopPublico(bool TopFavoritosPublico)
        {
            var user = await _userService.GetUsuarioActualAsync(HttpContext);
            if (user == null) return RedirectToAction("Login", "Usuarios");

            user.TopFavoritosPublico = TopFavoritosPublico;
            await _usuarioRepo.UpdateAsync(user);

            TempData["SettingsMessage"] = TopFavoritosPublico
                ? "Tu Top 3 ahora es público."
                : "Tu Top 3 ahora es privado.";

            return RedirectToAction("Index");
        }
    }
}