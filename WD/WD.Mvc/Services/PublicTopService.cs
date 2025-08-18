using WD.Mvc.Models;
using WD.Repository.Interfaces;

namespace WD.Mvc.Services
{
    // Servicio que maneja la logica de "Top 3 Publico" de los usuarios
    public class PublicTopService
    {
        private readonly IUsuarioRepository _usuarioRepo;    // Repositorio para acceder a usuarios
        private readonly FavoritosService _favoritosService; // Servicio de favoritos (ya maneja clima y top)

        public PublicTopService(IUsuarioRepository usuarioRepo, FavoritosService favoritosService)
        {
            _usuarioRepo = usuarioRepo;
            _favoritosService = favoritosService;
        }

        // Obtener top 3 favoritos de un usuario publico por ID
        public async Task<(bool ok, bool forbidden, string? error, string usuarioNombre, string symbol, List<FavoritoClimaViewModel> top)>
            GetTop3Async(int usuarioId, CancellationToken ct = default)
        {
            // Buscar usuario por ID
            var usuario = await _usuarioRepo.GetByIdAsync(usuarioId);
            if (usuario is null)
                return (false, false, "Usuario no encontrado.", string.Empty, string.Empty, new List<FavoritoClimaViewModel>());

            // Revisar si tiene TopFavoritosPublico activado
            if (!usuario.TopFavoritosPublico)
                return (false, true, null, string.Empty, string.Empty, new List<FavoritoClimaViewModel>());

            // Determinar unidades de temperatura
            var (units, symbol) = ResolveUnits(usuario.UnidadTemperatura);

            // Obtener top 3 favoritos del usuario
            var top3 = await _favoritosService.GetTopFavoritosAsync(usuario.IdUsuario, 3, units, ct);

            return (true, false, null, usuario.Nombre, symbol, top3);
        }

        // Explorar top 3 de todos los usuarios que tienen TopFavoritosPublico activado
        public async Task<List<PublicUserTopViewModel>> ExploreAsync(CancellationToken ct = default)
        {
            // Obtener todos los usuarios publicos
            var usuarios = (await _usuarioRepo.GetAllAsync())
                .Where(u => u.TopFavoritosPublico)
                .ToList();

            var model = new List<PublicUserTopViewModel>(usuarios.Count);

            // Para cada usuario publico, obtener su top 3 favoritos y crear el ViewModel
            foreach (var u in usuarios)
            {
                var (units, symbol) = ResolveUnits(u.UnidadTemperatura);
                var top = await _favoritosService.GetTopFavoritosAsync(u.IdUsuario, 3, units, ct);

                model.Add(new PublicUserTopViewModel
                {
                    UsuarioId = u.IdUsuario,
                    Nombre = u.Nombre,
                    Units = units,
                    UnitSymbol = symbol,
                    Top = top
                });
            }

            return model;
        }

        // Metodo auxiliar para determinar unidades y simbolo de temperatura
        private static (string units, string symbol) ResolveUnits(string? unidadTemperatura)
        {
            var units = (unidadTemperatura == "F") ? "imperial" : "metric";
            var symbol = (units == "imperial") ? "°F" : "°C";
            return (units, symbol);
        }
    }
}
