using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD.Models;

namespace WD.Repository.Interfaces;

// Define la interfaz para el repositorio de usuarios con operaciones asincronas
public interface IUsuarioRepository
{
    // Obtiene un usuario por su Id, retorna null si no existe
    Task<Usuario?> GetByIdAsync(int id);

    // Obtiene todos los usuarios registrados
    Task<IEnumerable<Usuario>> GetAllAsync();

    // Agrega un nuevo usuario a la base de datos
    Task AddAsync(Usuario usuario);

    // Actualiza un usuario existente
    Task UpdateAsync(Usuario usuario);

    // Elimina un usuario por su Id
    Task DeleteAsync(int id);
}

