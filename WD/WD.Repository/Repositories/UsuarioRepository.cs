using Microsoft.EntityFrameworkCore;
using WD.Data.DB;
using WD.Models;
using WD.Repository.Interfaces;

namespace WD.Repository.Repositories;

// Implementa la interfaz IUsuarioRepository para manejar datos de usuarios usando Entity Framework
public class UsuarioRepository : IUsuarioRepository
{
    // Contexto de la base de datos para acceder a las tablas
    private readonly WeatherDbContext _context;

    // Constructor que recibe el contexto via inyeccion de dependencias
    public UsuarioRepository(WeatherDbContext context) => _context = context;

    // Obtiene un usuario por su Id incluyendo sus favoritos relacionados
    public async Task<Usuario?> GetByIdAsync(int id) =>
        await _context.Usuarios
            .Include(u => u.Favoritos)          // Incluye la relacion Favoritos
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

    // Obtiene todos los usuarios con sus favoritos relacionados
    public async Task<IEnumerable<Usuario>> GetAllAsync() =>
        await _context.Usuarios
            .Include(u => u.Favoritos)          // Incluye la relacion Favoritos
            .ToListAsync();

    // Agrega un nuevo usuario y guarda cambios en la base de datos
    public async Task AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    // Actualiza un usuario existente y guarda cambios
    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    // Elimina un usuario por Id si existe y guarda cambios
    public async Task DeleteAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
