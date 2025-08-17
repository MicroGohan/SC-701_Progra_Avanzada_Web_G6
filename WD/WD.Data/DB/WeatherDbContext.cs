using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WD.Models;

namespace WD.Data.DB;

public partial class WeatherDbContext : DbContext
{
    public WeatherDbContext()
    {
    }

    public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning Para proteger informacion sensible como cadenas de conexion, se recomienda moverla a configuracion externa
        => optionsBuilder.UseSqlServer("Server=localhost;Database=WeatherDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito).HasName("PK__favorito__78F875AE89CD8F09");

            entity.ToTable("favorito");

            entity.Property(e => e.IdFavorito).HasColumnName("id_favorito");

            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .HasColumnName("ciudad");

            entity.Property(e => e.FechaAgregado)
                .HasDefaultValueSql("(getdate())") 
                .HasColumnName("fecha_agregado");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasColumnName("pais");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__favorito__id_usu__3C69FB99");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__usuario__4E3E04ADE1EA58A0");

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Email, "UQ__usuario__AB6E6164252565AF").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())") 
                .HasColumnName("fecha_registro");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");

            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");

            entity.Property(e => e.Continente)
                .HasMaxLength(50)
                .HasColumnName("continente");

            entity.Property(e => e.UnidadTemperatura)
                .HasMaxLength(1)
                .HasDefaultValue("C")
                .HasColumnName("unidad_temperatura");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
