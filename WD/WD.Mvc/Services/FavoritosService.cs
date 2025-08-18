using WD.Mvc.Models;
using WD.Repository.Interfaces;
using WD.Models;

namespace WD.Mvc.Services
{
    public class FavoritosService
    {
        private readonly IFavoritoRepository _favoritoRepo;
        private readonly WeatherApiClient _weatherApi;

        public FavoritosService(IFavoritoRepository favoritoRepo, WeatherApiClient weatherApi)
        {
            _favoritoRepo = favoritoRepo;
            _weatherApi = weatherApi;
        }
        /// <summary>
        public async Task<List<FavoritoClimaViewModel>> GetTop5FavoritosAsync(int usuarioId, string units = "metric", CancellationToken ct = default)
        {
            var favoritos = await _favoritoRepo.GetTop5ByUserAsync(usuarioId, ct);
            return await EnriquecerConClimaAsync(favoritos, units, ct);
        }

        public async Task<List<FavoritoClimaViewModel>> GetTopFavoritosAsync(int usuarioId, int count, string units = "metric", CancellationToken ct = default)
        {
            var favoritosOrdenados = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            var top = favoritosOrdenados.Take(count);
            return await EnriquecerConClimaAsync(top, units, ct);
        }

        public async Task<List<FavoritoClimaViewModel>> GetFavoritosConClimaAsync(int usuarioId, string units, CancellationToken ct = default)
        {
            var favoritos = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            return await EnriquecerConClimaAsync(favoritos, units, ct);
        }

        public async Task<(bool agregado, Favorito? favorito)> TryAddFavoritoAsync(int usuarioId, string ciudad, string pais, CancellationToken ct = default)
        {
            var existentes = await _favoritoRepo.GetByUserOrderedAsync(usuarioId, ct);
            if (existentes.Any(f => f.Ciudad == ciudad && f.Pais == pais))
                return (false, null);

            var favorito = new Favorito
            {
                IdUsuario = usuarioId,
                Ciudad = ciudad,
                Pais = pais,
                FechaAgregado = DateOnly.FromDateTime(DateTime.Now)
            };
            await _favoritoRepo.AddAsync(favorito, ct);
            return (true, favorito);
        }

        public async Task<bool> TryDeleteAsync(int usuarioId, int idFavorito, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            await _favoritoRepo.DeleteAsync(fav, ct);
            return true;
        }

        public async Task<bool> TryUpdatePrioridadAsync(int usuarioId, int idFavorito, string prioridad, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            fav.Prioridad = prioridad;
            await _favoritoRepo.UpdateAsync(fav, ct);
            return true;
        }
        
        public async Task<bool> TryUpdateDescripcionAsync(int usuarioId, int idFavorito, string descripcion, CancellationToken ct = default)
        {
            var fav = await _favoritoRepo.GetByIdForUserAsync(idFavorito, usuarioId, ct);
            if (fav == null) return false;
            fav.Descripcion = descripcion;
            await _favoritoRepo.UpdateAsync(fav, ct);
            return true;
        }

        private async Task<List<FavoritoClimaViewModel>> EnriquecerConClimaAsync(IEnumerable<Favorito> favoritos, string units, CancellationToken ct)
        {
            var salida = new List<FavoritoClimaViewModel>();
            foreach (var fav in favoritos)
            {
                var results = await _weatherApi.SearchAsync($"{fav.Ciudad},{fav.Pais}", 1, units, ct);
                var clima = results.FirstOrDefault();

                salida.Add(new FavoritoClimaViewModel
                {
                    Favorito = fav,
                    WeatherDescription = clima?.WeatherDescription,
                    Temperatura = clima?.Temperature,
                    Humedad = clima?.Humidity
                });
            }
            return salida;
        }
    }
}
