using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace alquiler_vehiculos_g11.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<CategoriaVehiculo> CategoriasVehiculo { get; set; } = null!;
    public DbSet<Vehiculo> Vehiculos { get; set; } = null!;
    public DbSet<Alquiler> Alquileres { get; set; } = null!;
    public DbSet<Pago> Pagos { get; set; } = null!;
    public DbSet<Imagen> Imagenes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .HasQueryFilter(c => c.Activo);

        modelBuilder.Entity<Alquiler>()
            .HasOne(alquiler => alquiler.CreadoPorUsuario)
            .WithMany()
            .HasForeignKey(alquiler => alquiler.CreadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Alquiler>()
            .HasOne(alquiler => alquiler.CerradoPorUsuario)
            .WithMany()
            .HasForeignKey(alquiler => alquiler.CerradoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Pago>()
            .HasOne(pago => pago.CreadoPorUsuario)
            .WithMany()
            .HasForeignKey(pago => pago.CreadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(pago => pago.AnuladoPorUsuario)
            .WithMany()
            .HasForeignKey(pago => pago.AnuladoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Alquiler>()
            .HasOne(alquiler => alquiler.Cliente)
            .WithMany()
            .HasForeignKey(alquiler => alquiler.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Alquiler>()
            .HasOne(alquiler => alquiler.Vehiculo)
            .WithMany(vehiculo => vehiculo.Alquileres)
            .HasForeignKey(alquiler => alquiler.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pago>()
            .HasOne(pago => pago.Alquiler)
            .WithMany(alquiler => alquiler.Pagos)
            .HasForeignKey(pago => pago.AlquilerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Imagen>()
            .HasOne(imagen => imagen.Vehiculo)
            .WithMany()
            .HasForeignKey(imagen => imagen.VehiculoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Imagen>()
            .HasOne(imagen => imagen.Alquiler)
            .WithMany(alquiler => alquiler.Imagenes)
            .HasForeignKey(imagen => imagen.AlquilerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Alquiler>()
            .Property(alquiler => alquiler.PrecioAcordado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Alquiler>()
            .Property(alquiler => alquiler.Multa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Alquiler>()
            .Property(alquiler => alquiler.ImporteTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Pago>()
            .Property(pago => pago.Importe)
            .HasPrecision(18, 2);
    }
}
