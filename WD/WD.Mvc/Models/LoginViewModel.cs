namespace WD.Mvc.Models;

// Define una clase que sirve como modelo de datos para el formulario de login
public class LoginViewModel
{
    // Campo para el email ingresado por el usuario en el formulario
    public string Email { get; set; } = string.Empty;

    // Campo para la contrasena ingresada por el usuario en el formulario
    public string Password { get; set; } = string.Empty;
}
