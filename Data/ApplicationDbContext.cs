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

    // Tablas de Dominio - Exclusivas de la Etapa 1
    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<CategoriaVehiculo> CategoriasVehiculo { get; set; } = null!;
    public DbSet<Vehiculo> Vehiculos { get; set; } = null!;
    

    //Query filter
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .HasQueryFilter(c => c.Activo); // Mostraba los clientes inactivos, ahora solo muestra los activos.
    }
}
