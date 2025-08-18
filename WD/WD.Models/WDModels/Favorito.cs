using System;
using System.Collections.Generic;

namespace WD.Models;

// Clase parcial para representar la tabla Favorito en la base de datos
public partial class Favorito
{
    // Identificador unico del favorito
    public int IdFavorito { get; set; }

    // Llave foranea que referencia al usuario
    public int IdUsuario { get; set; }

    // Nombre de la ciudad guardada como favorita
    public string Ciudad { get; set; } = null!;

    // Nombre del pais correspondiente
    public string Pais { get; set; } = null!;

    // Fecha en que se agrego el favorito
    public DateOnly FechaAgregado { get; set; }

    // Campo opcional para una descripcion extra
    public string? Descripcion { get; set; }

    // Propiedad de navegacion para acceder al objeto Usuario relacionado
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    // Campo para asignar prioridad, por defecto es "bajo"
    public string Prioridad { get; set; } = "bajo";
}
