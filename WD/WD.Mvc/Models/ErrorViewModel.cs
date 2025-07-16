namespace WD.Mvc.Models
{
    // Clase que representa el modelo para mostrar errores en las vistas
    public class ErrorViewModel
    {
        // Identificador unico de la solicitud HTTP, util para rastrear errores
        public string? RequestId { get; set; }

        // Propiedad calculada que indica si se debe mostrar el RequestId
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
