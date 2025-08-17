using Microsoft.EntityFrameworkCore;
using WD.Data.DB;
using WD.Models;
using WD.Repository.Interfaces;

namespace WD.Repository.Repositories;

public class FavoritoRepository : IFavoritoRepository
{
    private readonly WeatherDbContext _context;
    public FavoritoRepository(WeatherDbContext context) => _context = context;

    public async Task<List<Favorito>> GetByUserOrderedAsync(int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .Where(f => f.IdUsuario == usuarioId)
            .OrderBy(f => f.Prioridad == "bajo" ? 3 :
                          f.Prioridad == "medio" ? 2 :
                          f.Prioridad == "alto" ? 1 : 0)
            .ThenByDescending(f => f.FechaAgregado)
            .ToListAsync(ct);

    public async Task<List<Favorito>> GetTop5ByUserAsync(int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .Where(f => f.IdUsuario == usuarioId)
            .OrderBy(f => f.Prioridad == "alto" ? 0 : f.Prioridad == "medio" ? 1 : 2)
            .ThenByDescending(f => f.FechaAgregado)
            .Take(5)
            .ToListAsync(ct);

    public async Task<Favorito?> GetByIdForUserAsync(int idFavorito, int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .FirstOrDefaultAsync(f => f.IdFavorito == idFavorito && f.IdUsuario == usuarioId, ct);

    public async Task AddAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Add(favorito);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Update(favorito);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync(ct);
    }
}