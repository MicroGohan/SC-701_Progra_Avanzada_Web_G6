using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WD.Mvc.Models
{
    public class FavoritoClimaViewModel
    {
        public WD.Models.Favorito Favorito { get; set; } = null!;
        public string? WeatherDescription { get; set; }
        public double? Temperatura { get; set; }
        public int? Humedad { get; set; }
    }
}
