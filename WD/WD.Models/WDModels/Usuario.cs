using System;
using System.Collections.Generic;

namespace WD.Models;

// Define una clase parcial llamada Usuario
public partial class Usuario
{
    // Identificador unico del usuario (clave primaria en la base de datos)
    public int IdUsuario { get; set; }

    // Nombre del usuario (no admite valores nulos)
    public string Nombre { get; set; } = null!;

    // Email del usuario (no admite valores nulos)
    public string Email { get; set; } = null!;

    // Password del usuario (no admite valores nulos)
    public string Password { get; set; } = null!;

    // Fecha en que el usuario se registro, puede ser nula
    public DateOnly? FechaRegistro { get; set; }

    // Preferencias del usuario
    public string Continente { get; set; } = "America"; // Continente
    public string UnidadTemperatura { get; set; } = "C"; // Solo "C" o "F"

    // Coleccion de favoritos asociados a este usuario (relacion uno a muchos)
    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
