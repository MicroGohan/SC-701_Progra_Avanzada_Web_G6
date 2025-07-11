using Microsoft.AspNetCore.Mvc;
using WD.Mvc.Models;
using WD.Data.Models;
using WD.Repository.Interfaces;

namespace WD.Mvc.Controllers;

public class UsuariosController : Controller
{
    private readonly IUsuarioRepository _usuarioRepo;

    public UsuariosController(IUsuarioRepository usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existe = (await _usuarioRepo.GetAllAsync()).Any(u => u.Email == model.Email);
        if (existe)
        {
            ModelState.AddModelError("Email", "El email ya está registrado.");
            return View(model);
        }

        var usuario = new Usuario
        {
            Nombre = model.Nombre,
            Email = model.Email,
            Password = model.Password
        };

        await _usuarioRepo.AddAsync(usuario);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var usuario = (await _usuarioRepo.GetAllAsync())
            .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Usuarios");
    }
}