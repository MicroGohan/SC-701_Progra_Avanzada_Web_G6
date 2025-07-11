using Microsoft.EntityFrameworkCore;
using WD.Data;
using WD.Data.Models;
using WD.Models;
using WD.Repository.Interfaces;

namespace WD.Repository.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly WeatherDbContext _context;
    public UsuarioRepository(WeatherDbContext context) => _context = context;

    public async Task<Usuario?> GetByIdAsync(int id) =>
        await _context.Usuarios.Include(u => u.Favoritos).FirstOrDefaultAsync(u => u.IdUsuario == id);

    public async Task<IEnumerable<Usuario>> GetAllAsync() =>
        await _context.Usuarios.Include(u => u.Favoritos).ToListAsync();

    public async Task AddAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

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
