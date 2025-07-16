using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WD.Models;

namespace WD.Data.DB;

// Define la clase del contexto de base de datos que extiende de DbContext
public partial class WeatherDbContext : DbContext
{
    // Constructor vacio necesario para ciertos escenarios como migraciones
    public WeatherDbContext()
    {
    }

    // Constructor que recibe opciones para configurar el contexto (por ejemplo, el proveedor y cadena de conexion)
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    // Representa la tabla "favorito" como un conjunto de entidades Favorito
    public virtual DbSet<Favorito> Favoritos { get; set; }

    // Representa la tabla "usuario" como un conjunto de entidades Usuario
    public virtual DbSet<Usuario> Usuarios { get; set; }

    // Metodo llamado al configurar el contexto (por ejemplo, definir el proveedor de base de datos)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning Para proteger informacion sensible como cadenas de conexion, se recomienda moverla a configuracion externa
        => optionsBuilder.UseSqlServer("Server=Max;Database=WeatherDB;Trusted_Connection=True;TrustServerCertificate=True;");

    // Metodo para configurar el modelo de EF Core mediante el ModelBuilder
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuracion de la entidad Favorito
        modelBuilder.Entity<Favorito>(entity =>
        {
            // Define la clave primaria de la tabla
            entity.HasKey(e => e.IdFavorito).HasName("PK__favorito__78F875AE89CD8F09");

            // Define el nombre real de la tabla en la base de datos
            entity.ToTable("favorito");

            // Mapea cada propiedad al nombre de columna correspondiente en la base de datos
            entity.Property(e => e.IdFavorito).HasColumnName("id_favorito");

            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");

            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("(getdate())") // Valor por defecto: fecha y hora actual
                .HasColumnName("fecha_agregado");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasColumnName("pais");

            // Define la relacion con la entidad Usuario (clave foranea)
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__favorito__id_usu__3C69FB99");
        });

        // Configuracion de la entidad Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            // Define la clave primaria
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario__4E3E04ADE1EA58A0");

            // Define el nombre de la tabla
            entity.ToTable("usuario");

            // Crea un indice unico sobre el campo email
            entity.HasIndex(e => e.Email, "UQ__usuario__AB6E6164252565AF").IsUnique();

            // Mapea las propiedades con sus respectivos nombres de columna
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())") // Valor por defecto: fecha y hora actual
                .HasColumnName("fecha_registro");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
        });

        // Llama a metodo parcial que permite configuraciones adicionales si se define en otro archivo
        OnModelCreatingPartial(modelBuilder);
    }

    // Metodo parcial que puede ser implementado en otro archivo para extender configuraciones del modelo
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
