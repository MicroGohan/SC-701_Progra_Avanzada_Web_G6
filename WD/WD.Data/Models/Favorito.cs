using System;
using System.Collections.Generic;

namespace WD.Data.Models;

public partial class Favorito
{
    public int IdFavorito { get; set; }

    public int IdUsuario { get; set; }

    public string Ciudad { get; set; } = null!;

    public string? Pais { get; set; }

    public DateOnly? FechaAgregado { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
