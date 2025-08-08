using Microsoft.AspNetCore.Mvc;
using WD.Data.DB;
using WD.Mvc.Models;

namespace WD.Mvc.Services
{
    public class FavoritosService
    {
        private readonly WeatherDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public FavoritosService(WeatherDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<FavoritoClimaViewModel>> GetTop5FavoritosAsync(int usuarioId)
        {
            var favoritos = _context.Favoritos
                .Where(f => f.IdUsuario == usuarioId)
                .OrderBy(f => f.Prioridad == "alto" ? 0 : f.Prioridad == "medio" ? 1 : 2)
                .ThenByDescending(f => f.FechaAgregado)
                .Take(5)
                .ToList();

            var favoritosClima = new List<FavoritoClimaViewModel>();
            var client = _httpClientFactory.CreateClient();

            foreach (var fav in favoritos)
            {
                var url = $"https://localhost:7215/api/weather/search?q={Uri.EscapeDataString(fav.Ciudad + "," + fav.Pais)}&limit=1";
                var response = await client.GetAsync(url);

                string? weatherDescription = null;
                double? temperatura = null;
                int? humedad = null;

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var results = System.Text.Json.JsonSerializer.Deserialize<List<WD.Models.WDModels.WeatherResult>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var clima = results?.FirstOrDefault();
                    if (clima != null)
                    {
                        weatherDescription = clima.WeatherDescription;
                        temperatura = clima.Temperature;
                        humedad = clima.Humidity;
                    }
                }

                favoritosClima.Add(new FavoritoClimaViewModel
                {
                    Favorito = fav,
                    WeatherDescription = weatherDescription,
                    Temperatura = temperatura,
                    Humedad = humedad
                });
            }

            return favoritosClima;
        }
    }
}
