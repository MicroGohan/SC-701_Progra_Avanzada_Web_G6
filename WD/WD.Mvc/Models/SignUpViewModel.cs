namespace WD.Mvc.Models;

// Define un modelo para la vista de registro (SignUp)
public class SignUpViewModel
{
    // Nombre ingresado por el usuario en el formulario de registro
    public string Nombre { get; set; } = string.Empty;

    // Email ingresado por el usuario en el formulario de registro
    public string Email { get; set; } = string.Empty;

    // Contrasena ingresada por el usuario en el formulario de registro
    public string Password { get; set; } = string.Empty;
}
