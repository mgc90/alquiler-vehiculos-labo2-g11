using alquiler_vehiculos_g11.Models;
using Microsoft.AspNetCore.Identity;

namespace alquiler_vehiculos_g11.Data;

public static class DatosIniciales
{
    public static async Task InicializarAsync(IServiceProvider servicios)
    {
        var roleManager = servicios.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = servicios.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Administrador", "Empleado" };

        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(rol));
            }
        }

        await CrearSiNoExisteAsync(userManager, "admin@drivecontrol.com", "Admin123!",
            "Usuario", "Administrador", "Administrador");
        await CrearSiNoExisteAsync(userManager, "empleado@drivecontrol.com", "Empleado123!",
            "Usuario", "Empleado", "Empleado");
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
