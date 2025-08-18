using WD.Models;

namespace WD.Repository.Interfaces;

// Interfaz que define las operaciones CRUD y consultas de Favoritos en la BD
public interface IFavoritoRepository
{
    // Obtiene todos los favoritos de un usuario ordenados (por fecha o prioridad)
    Task<List<Favorito>> GetByUserOrderedAsync(int usuarioId, CancellationToken ct = default);

    // Obtiene los 5 favoritos principales de un usuario
    Task<List<Favorito>> GetTop5ByUserAsync(int usuarioId, CancellationToken ct = default);

    // Obtiene un favorito especifico de un usuario por su ID
    Task<Favorito?> GetByIdForUserAsync(int idFavorito, int usuarioId, CancellationToken ct = default);

    // Agrega un nuevo favorito a la base de datos
    Task AddAsync(Favorito favorito, CancellationToken ct = default);

    // Actualiza un favorito existente
    Task UpdateAsync(Favorito favorito, CancellationToken ct = default);

    // Elimina un favorito de la base de datos
    Task DeleteAsync(Favorito favorito, CancellationToken ct = default);
}
