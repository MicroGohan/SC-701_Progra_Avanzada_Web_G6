using Microsoft.AspNetCore.Mvc;
using WD.Models;
using WD.Mvc.Models;
using WD.Repository.Interfaces;

namespace WD.Mvc.Controllers;

// Define un controlador MVC llamado UsuariosController
public class UsuariosController : Controller
{
    // Repositorio para acceder a datos de usuarios
    private readonly IUsuarioRepository _usuarioRepo;

    // Constructor que recibe inyeccion del repositorio de usuarios
    public UsuariosController(IUsuarioRepository usuarioRepo)
    {
        _usuarioRepo = usuarioRepo;
    }

    // Metodo que responde a peticiones GET para mostrar la vista de registro (SignUp)
    [HttpGet]
    public IActionResult SignUp()
    {
        // Devuelve la vista SignUp vacia
        return View();
    }

    // Metodo que responde a peticiones POST para procesar el registro de usuario
    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        // Verifica si el modelo recibido es valido
        if (!ModelState.IsValid)
            return View(model);

        // Consulta si ya existe un usuario con el mismo email
        var existe = (await _usuarioRepo.GetAllAsync()).Any(u => u.Email == model.Email);

        if (existe)
        {
            // Si existe, agrega un error al ModelState y muestra nuevamente la vista
            ModelState.AddModelError("Email", "El email ya esta registrado.");
            return View(model);
        }

        // Crea un nuevo objeto Usuario con los datos del modelo
        var usuario = new Usuario
        {
            Nombre = model.Nombre,
            Email = model.Email,
            Password = model.Password
        };

        // Guarda el nuevo usuario en la base de datos
        await _usuarioRepo.AddAsync(usuario);

        // Redirige al Login despues de registrarse
        return RedirectToAction("Login");
    }

    // Metodo que responde a peticiones GET para mostrar la vista de login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Metodo que responde a peticiones POST para procesar el login del usuario
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Verifica si el modelo recibido es valido
        if (!ModelState.IsValid)
            return View(model);

        // Busca un usuario con el email y password proporcionados
        var usuario = (await _usuarioRepo.GetAllAsync())
            .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

        if (usuario == null)
        {
            // Si no se encuentra usuario, agrega un error al ModelState
            ModelState.AddModelError(string.Empty, "Email o contrasena incorrectos.");
            return View(model);
        }

        // Guarda el Id y Nombre del usuario en la sesion
        HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
        HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

        // Redirige a la accion Index del HomeController
        return RedirectToAction("Index", "Home");
    }

    // Metodo que responde a peticiones POST para cerrar sesion
    [HttpPost]
    public IActionResult Logout()
    {
        // Limpia todas las variables de sesion
        HttpContext.Session.Clear();

        // Redirige al login
        return RedirectToAction("Login", "Usuarios");
    }
}
