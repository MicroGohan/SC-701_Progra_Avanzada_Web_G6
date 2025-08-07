using System;
using System.Collections.Generic;

namespace WD.Models;

public partial class Favorito
{
    public int IdFavorito { get; set; }

    public int IdUsuario { get; set; }

    public string Ciudad { get; set; } = null!;

    public string Pais { get; set; } = null!;

    public DateOnly FechaAgregado { get; set; }
    public string? Descripcion { get; set; }
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
    public string Prioridad { get; set; } = "bajo";
}
