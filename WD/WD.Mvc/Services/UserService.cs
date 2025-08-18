using Microsoft.AspNetCore.Http;
using WD.Models;
using WD.Repository.Interfaces;
using WD.Mvc.Models;

namespace WD.Mvc.Services
{
    // Servicio que maneja la logica de usuario: autenticacion, perfil y preferencias
    public class UserService
    {
        private readonly IUsuarioRepository _usuarioRepo; // Repositorio para acceder a los datos de usuario

        public UserService(IUsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        // Obtener el Id del usuario desde la sesion
        public int? GetUsuarioId(HttpContext http)
        {
            var s = http.Session.GetString("UsuarioId");
            return int.TryParse(s, out var id) ? id : null;
        }

        // Obtener el objeto Usuario completo actual desde la sesion
        public async Task<Usuario?> GetUsuarioActualAsync(HttpContext http)
        {
            var id = GetUsuarioId(http);
            return id.HasValue ? await _usuarioRepo.GetByIdAsync(id.Value) : null;
        }

        // Obtener unidades de temperatura y simbolo
        public async Task<(string units, string symbol)> GetTemperatureUnitsAsync(HttpContext http)
        {
            var user = await GetUsuarioActualAsync(http);
            return ResolveUnits(user?.UnidadTemperatura);
        }

        // Actualiza el nombre en la sesion
        public void RefreshSessionNombre(HttpContext http, string nombre) =>
            http.Session.SetString("UsuarioNombre", nombre);

        // Verificar si un email ya existe en la base de datos
        public async Task<bool> EmailExistsAsync(string email)
        {
            var all = await _usuarioRepo.GetAllAsync();
            return all.Any(u => u.Email == email);
        }

        // Intentar registrar un nuevo usuario
        public async Task<(bool ok, string? error)> TrySignUpAsync(SignUpViewModel model)
        {
            if (await EmailExistsAsync(model.Email))
                return (false, "El email ya esta registrado.");

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Password = model.Password
            };

            await _usuarioRepo.AddAsync(usuario);
            return (true, null);
        }

        // Buscar usuario por email y password
        public async Task<Usuario?> FindByCredentialsAsync(string email, string password)
        {
            var all = await _usuarioRepo.GetAllAsync();
            return all.FirstOrDefault(u => u.Email == email && u.Password == password);
        }

        // Iniciar sesion guardando datos en Session
        public void SignIn(HttpContext http, Usuario usuario)
        {
            http.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
            http.Session.SetString("UsuarioNombre", usuario.Nombre);
        }

        // Cerrar sesion y limpiar Session
        public void SignOut(HttpContext http)
        {
            http.Session.Clear();
        }

        // Actualizar datos de perfil del usuario
        public async Task<(bool ok, string? error)> UpdateProfileAsync(HttpContext http, UserSettingsViewModel model)
        {
            var user = await GetUsuarioActualAsync(http);
            if (user == null) return (false, "No autenticado.");

            if (string.IsNullOrWhiteSpace(model.Nombre))
                return (false, "El nombre es requerido.");

            user.Nombre = model.Nombre.Trim();
            user.Continente = string.IsNullOrWhiteSpace(model.Continente) ? null : model.Continente.Trim();
            user.UnidadTemperatura = (model.UnidadTemperatura == "F") ? "F" : "C";

            await _usuarioRepo.UpdateAsync(user);
            RefreshSessionNombre(http, user.Nombre);

            return (true, null);
        }

        // Cambiar contraseña del usuario
        public async Task<(bool ok, string? error)> ChangePasswordAsync(HttpContext http, string? actual, string? nueva, string? confirmacion)
        {
            var user = await GetUsuarioActualAsync(http);
            if (user == null) return (false, "No autenticado.");

            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(nueva) || string.IsNullOrWhiteSpace(confirmacion))
                return (false, "Complete todos los campos de contraseña.");

            if (!string.Equals(user.Password, actual))
                return (false, "La contraseña actual es incorrecta.");

            if (!string.Equals(nueva, confirmacion))
                return (false, "La confirmacion no coincide.");

            user.Password = nueva;
            await _usuarioRepo.UpdateAsync(user);

            return (true, null);
        }

        // Actualizar visibilidad del Top 3 publico
        public async Task<(bool ok, string? error)> UpdateTopPublicoAsync(HttpContext http, bool topFavoritosPublico)
        {
            var user = await GetUsuarioActualAsync(http);
            if (user == null) return (false, "No autenticado.");

            user.TopFavoritosPublico = topFavoritosPublico;
            await _usuarioRepo.UpdateAsync(user);
            return (true, null);
        }

        // Convertir letra de unidad a string para API y simbolo para UI
        public static (string units, string symbol) ResolveUnits(string? unidadTemperatura)
        {
            var units = (unidadTemperatura == "F") ? "imperial" : "metric";
            var symbol = (units == "imperial") ? "°F" : "°C";
            return (units, symbol);
        }
    }
}
