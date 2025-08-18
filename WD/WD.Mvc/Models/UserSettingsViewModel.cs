namespace WD.Mvc.Models;

// ViewModel que representa la configuración del usuario
public class UserSettingsViewModel
{
    // Perfil
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; /
    public string? Continente { get; set; }
    public string UnidadTemperatura { get; set; } = "C"; 
    public bool TopFavoritosPublico { get; set; }
    public string? PasswordActual { get; set; }
    public string? PasswordNuevo { get; set; }
    public string? PasswordConfirmacion { get; set; }
}