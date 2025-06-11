using System;
using System.Collections.Generic;

namespace WD.Data.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly? FechaRegistro { get; set; }

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
