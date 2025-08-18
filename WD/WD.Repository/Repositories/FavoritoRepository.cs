using Microsoft.EntityFrameworkCore;
using WD.Data.DB;
using WD.Models;
using WD.Repository.Interfaces;

namespace WD.Repository.Repositories;

// Implementacion concreta de IFavoritoRepository usando EF Core
public class FavoritoRepository : IFavoritoRepository
{
    private readonly WeatherDbContext _context;
    public FavoritoRepository(WeatherDbContext context) => _context = context;

    // Obtiene todos los favoritos de un usuario, ordenados por prioridad (alto, medio, bajo) y fecha descendente
    public async Task<List<Favorito>> GetByUserOrderedAsync(int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .Where(f => f.IdUsuario == usuarioId)
            .OrderBy(f => f.Prioridad == "bajo" ? 3 :
                          f.Prioridad == "medio" ? 2 :
                          f.Prioridad == "alto" ? 1 : 0)
            .ThenByDescending(f => f.FechaAgregado)
            .ToListAsync(ct);

    // Obtiene los 5 favoritos principales de un usuario, priorizando los mas importantes
    public async Task<List<Favorito>> GetTop5ByUserAsync(int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .Where(f => f.IdUsuario == usuarioId)
            .OrderBy(f => f.Prioridad == "alto" ? 0 : f.Prioridad == "medio" ? 1 : 2)
            .ThenByDescending(f => f.FechaAgregado)
            .Take(5)
            .ToListAsync(ct);

    // Obtiene un favorito especifico de un usuario por ID
    public async Task<Favorito?> GetByIdForUserAsync(int idFavorito, int usuarioId, CancellationToken ct = default) =>
        await _context.Favoritos
            .FirstOrDefaultAsync(f => f.IdFavorito == idFavorito && f.IdUsuario == usuarioId, ct);

    // Agrega un nuevo favorito y guarda cambios
    public async Task AddAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Add(favorito);
        await _context.SaveChangesAsync(ct);
    }

    // Actualiza un favorito existente y guarda cambios
    public async Task UpdateAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Update(favorito);
        await _context.SaveChangesAsync(ct);
    }

    // Elimina un favorito y guarda cambios
    public async Task DeleteAsync(Favorito favorito, CancellationToken ct = default)
    {
        _context.Favoritos.Remove(favorito);
        await _context.SaveChangesAsync(ct);
    }
}
