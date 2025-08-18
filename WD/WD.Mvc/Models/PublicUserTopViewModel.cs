namespace WD.Mvc.Models
{
    public class PublicUserTopViewModel
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; } = "";
        public string Units { get; set; } = "metric";
        public string UnitSymbol { get; set; } = "°C";
        public List<FavoritoClimaViewModel> Top { get; set; } = new();
    }
}