using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using alquiler_vehiculos_g11.Models.Enums;

namespace alquiler_vehiculos_g11.Data;

public static class DatosIniciales
{
    public static async Task InicializarAsync(IServiceProvider servicios)
    {
        var roleManager = servicios.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = servicios.GetRequiredService<UserManager<ApplicationUser>>();
        var context = servicios.GetRequiredService<ApplicationDbContext>();

        string[] roles = { "Administrador", "Empleado" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(rol));
            }
        }
        // --------- Usuarios ----------
        await CrearSiNoExisteAsync(userManager, "admin@drivecontrol.com", "Admin123!",
            "Usuario", "Administrador", "Administrador");
        await CrearSiNoExisteAsync(userManager, "empleado@drivecontrol.com", "Empleado123!",
            "Usuario", "Empleado", "Empleado");


        // --------- Categorias de Vehiculos ----------
        if (!await context.CategoriasVehiculo.AnyAsync())
        {
            var categorias = new List<CategoriaVehiculo>
            {
                new CategoriaVehiculo
                {
                    Nombre = "Compacto",
                    Descripcion = "Vehículo ágil e ideal para desplazamientos urbanos.",
                    PrecioDiario = 15000,
                    DepositoGarantia = 50000,
                    Activa = true
                },
                new CategoriaVehiculo
                {
                    Nombre = "SUV",
                    Descripcion = "Vehículo espacioso, ideal para viajes familiares o de larga distancia.",
                    PrecioDiario = 35000,
                    DepositoGarantia = 80000,
                    Activa = true
                }
            };
            await context.CategoriasVehiculo.AddRangeAsync(categorias);
            await context.SaveChangesAsync();
        }

        // --------- Clientes ----------
        if (!await context.Clientes.AnyAsync())
        {
            var clientes = new List<Cliente>
            {
                new Cliente
                {
                    Dni = "28999111",
                    Nombre = "Carlos",
                    Apellido = "Gómez",
                    FechaNacimiento = new DateTime(1982, 4, 15),
                    Telefono = "2664112233",
                    Email = "carlos.gomez@email.com",
                    NumeroLicencia = "LIC-28999111",
                    VencimientoLicencia = DateTime.Now.AddYears(2),
                    Activo = true
                },
                new Cliente
                {
                    Dni = "35444555",
                    Nombre = "Ana",
                    Apellido = "Martínez",
                    FechaNacimiento = new DateTime(1990, 8, 22),
                    Telefono = "2664556677",
                    Email = "ana.martinez@email.com",
                    NumeroLicencia = "LIC-35444555",
                    VencimientoLicencia = DateTime.Now.AddYears(1),
                    Activo = true
                }
            };
            await context.Clientes.AddRangeAsync(clientes);
            await context.SaveChangesAsync();
            }


            // --------- Vehiculos ----------
            if (!await context.Vehiculos.AnyAsync())
            {
                var catCompacto = await context.CategoriasVehiculo.FirstOrDefaultAsync(c => c.Nombre == "Compacto");
                var catSuv = await context.CategoriasVehiculo.FirstOrDefaultAsync(c => c.Nombre == "SUV");

                if (catCompacto != null && catSuv != null)
                {
                    var vehiculos = new List<Vehiculo>
                {
                    new Vehiculo
                    {
                        CategoriaVehiculoId = catCompacto.Id,
                        Patente = "AF123JK",
                        Marca = "Toyota",
                        Modelo = "Etios",
                        KilometrajeActual = 25000,
                        EstadoOperativo = EstadoOperativoVehiculo.Habilitado
                    },
                    new Vehiculo
                    {
                        CategoriaVehiculoId = catSuv.Id,
                        Patente = "AG987LM",
                        Marca = "Ford",
                        Modelo = "Territory",
                        KilometrajeActual = 12000,
                        EstadoOperativo = EstadoOperativoVehiculo.Habilitado
                    }
                };
                    await context.Vehiculos.AddRangeAsync(vehiculos);
                    await context.SaveChangesAsync();
                }
            }

    }

    private static async Task CrearSiNoExisteAsync(UserManager<ApplicationUser> userManager,
        string email, string password, string nombre, string apellido, string rol)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var usuario = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Nombre = nombre,
            Apellido = apellido
        };

        var resultado = await userManager.CreateAsync(usuario, password);
        if (resultado.Succeeded)
        {
            await userManager.AddToRoleAsync(usuario, rol);
        }
    }
}
