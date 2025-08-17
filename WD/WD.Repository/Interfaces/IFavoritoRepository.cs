using WD.Models;

namespace WD.Repository.Interfaces;

public interface IFavoritoRepository
{
    Task<List<Favorito>> GetByUserOrderedAsync(int usuarioId, CancellationToken ct = default);
    Task<List<Favorito>> GetTop5ByUserAsync(int usuarioId, CancellationToken ct = default);
    Task<Favorito?> GetByIdForUserAsync(int idFavorito, int usuarioId, CancellationToken ct = default);
    Task AddAsync(Favorito favorito, CancellationToken ct = default);
    Task UpdateAsync(Favorito favorito, CancellationToken ct = default);
    Task DeleteAsync(Favorito favorito, CancellationToken ct = default);
}