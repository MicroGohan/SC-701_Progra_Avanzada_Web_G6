using Microsoft.AspNetCore.Mvc;
using WD.Data.DB;
using WD.Mvc.Models;

namespace WD.Mvc.Controllers
{
    public class SettingsController : Controller
    {
        private readonly WeatherDbContext _context;

        public SettingsController(WeatherDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            if (user == null)
                return RedirectToAction("Login", "Usuarios");

            var vm = new UserSettingsViewModel
            {
                Nombre = user.Nombre,
                Email = user.Email,
                Continente = user.Continente,
                UnidadTemperatura = string.IsNullOrWhiteSpace(user.UnidadTemperatura) ? "C" : user.UnidadTemperatura
            };

            ViewBag.Continentes = GetContinentes();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(UserSettingsViewModel model)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            if (user == null)
                return RedirectToAction("Login", "Usuarios");

            // Validaciones simples
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                TempData["SettingsError"] = "El nombre es requerido.";
                ViewBag.Continentes = GetContinentes();
                return View("Index", model);
            }

            // Actualizar perfil
            user.Nombre = model.Nombre.Trim();
            user.Continente = string.IsNullOrWhiteSpace(model.Continente) ? null : model.Continente.Trim();
            user.UnidadTemperatura = (model.UnidadTemperatura == "F") ? "F" : "C";

            _context.SaveChanges();

            // Refrescar nombre en sesion para el header
            HttpContext.Session.SetString("UsuarioNombre", user.Nombre);

            TempData["SettingsMessage"] = "Preferencias actualizadas correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(UserSettingsViewModel model)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr))
                return RedirectToAction("Login", "Usuarios");

            int usuarioId = int.Parse(usuarioIdStr);
            var user = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == usuarioId);
            if (user == null)
                return RedirectToAction("Login", "Usuarios");

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
            _context.SaveChanges();

            TempData["SettingsMessage"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index");
        }

        private static List<string> GetContinentes() => new()
        {
            "Africa", "America", "Asia", "Europa", "Oceania"
        };
    }
}