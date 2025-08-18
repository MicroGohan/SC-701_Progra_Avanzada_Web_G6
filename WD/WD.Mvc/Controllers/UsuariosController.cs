using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Models;
using WD.Mvc.Services;

namespace WD.Mvc.Controllers;

// Controlador para manejar usuarios (registro, login, logout)
public class UsuariosController : Controller
{
    private readonly UserService _userService;

    // Constructor con inyeccion de dependencia del servicio de usuarios
    public UsuariosController(UserService userService)
    {
        _userService = userService;
    }

    // Vista de registro de usuario
    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }

    // Procesa registro de usuario
    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        // Validar datos del modelo
        if (!ModelState.IsValid)
            return View(model);

        // Intentar registrar al usuario usando el servicio
        var (ok, error) = await _userService.TrySignUpAsync(model);

        // Si falla mostrar error en la vista
        if (!ok)
        {
            ModelState.AddModelError("Email", error ?? "No se pudo registrar el usuario.");
            return View(model);
        }

        // Si todo fue bien redirigir al login
        return RedirectToAction("Login");
    }

    // Vista de inicio de sesion
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Procesa inicio de sesion
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Validar datos del modelo
        if (!ModelState.IsValid)
            return View(model);

        // Buscar usuario por credenciales
        var usuario = await _userService.FindByCredentialsAsync(model.Email, model.Password);

        // Si no existe o las credenciales son incorrectas
        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        // Iniciar sesion (guardar cookies/claims)
        _userService.SignIn(HttpContext, usuario);

        // Redirigir a la pagina principal
        return RedirectToAction("Index", "Home");
    }

    // Cerrar sesion
    [HttpPost]
    public IActionResult Logout()
    {
        // Quitar autenticacion
        _userService.SignOut(HttpContext);

        // Redirigir al login
        return RedirectToAction("Login", "Usuarios");
    }
}
