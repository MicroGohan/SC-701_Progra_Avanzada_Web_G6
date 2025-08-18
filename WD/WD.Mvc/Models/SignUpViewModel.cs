namespace WD.Mvc.Models;

// ViewModel que representa el formulario de registro de un nuevo usuario
public class SignUpViewModel
{
    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
