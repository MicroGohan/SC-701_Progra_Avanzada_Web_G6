namespace WD.Mvc.Models;

public class UserSettingsViewModel
{
    // Perfil
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; // Solo lectura en UI
    public string? Continente { get; set; }
    public string UnidadTemperatura { get; set; } = "C"; // "C" o "F"

    // Cambio de contraseña
    public string? PasswordActual { get; set; }
    public string? PasswordNuevo { get; set; }
    public string? PasswordConfirmacion { get; set; }
}