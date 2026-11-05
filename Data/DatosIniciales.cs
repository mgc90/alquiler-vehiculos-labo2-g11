using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        var admin = await CrearSiNoExisteAsync(userManager, "admin@drivecontrol.com", "Admin123!",
            "Usuario", "Administrador", "Administrador");
        var empleado = await CrearSiNoExisteAsync(userManager, "empleado@drivecontrol.com", "Empleado123!",
            "Usuario", "Empleado", "Empleado");

        await AsegurarAvatarAsync(
            userManager,
            admin,
            "uploads/avatar_administrador.png"
        );

        await AsegurarAvatarAsync(
            userManager,
            empleado,
            "uploads/avatar_empleado.png"
        );

        await AsegurarCategoriasAsync(context);
        await AsegurarClientesAsync(context);
        await AsegurarVehiculosAsync(context);

        var categorias = await context.CategoriasVehiculo.IgnoreQueryFilters()
            .ToDictionaryAsync(c => c.Nombre, c => c);
        var clientes = await context.Clientes.IgnoreQueryFilters()
            .ToListAsync();
        var vehiculos = await context.Vehiculos.Include(v => v.CategoriaVehiculo)
            .ToListAsync();

        if (!await context.Alquileres.AnyAsync())
        {
            admin ??= await userManager.FindByEmailAsync("admin@drivecontrol.com");
            empleado ??= await userManager.FindByEmailAsync("empleado@drivecontrol.com");
            if (admin == null || empleado == null)
            {
                return;
            }

            await CrearAlquileresAsync(
                context, categorias, clientes, vehiculos, admin, empleado);
        }

        await AsegurarPortadasAsync(context, vehiculos);
    }

    public static async Task ReinicializarDemoAsync(IServiceProvider servicios)
    {
        var context = servicios.GetRequiredService<ApplicationDbContext>();

        var vehiculosDemo = await context.Vehiculos.IgnoreQueryFilters()
            .Where(v => PatsDemo().Contains(v.Patente))
            .ToListAsync();
        var vehiculosIds = vehiculosDemo.Select(v => v.Id).ToList();

        var alquileresDemo = await context.Alquileres.IgnoreQueryFilters()
            .Where(a => vehiculosIds.Contains(a.VehiculoId))
            .ToListAsync();
        var alquileresIds = alquileresDemo.Select(a => a.Id).ToList();

        var imagenesDemo = await context.Imagenes.IgnoreQueryFilters()
            .Where(i => vehiculosIds.Contains(i.VehiculoId))
            .ToListAsync();
        if (imagenesDemo.Any())
        {
            context.Imagenes.RemoveRange(imagenesDemo);
        }

        var pagosDemo = await context.Pagos
            .Where(p => alquileresIds.Contains(p.AlquilerId))
            .ToListAsync();
        if (pagosDemo.Any())
        {
            context.Pagos.RemoveRange(pagosDemo);
        }

        var categoriasDemo = await context.CategoriasVehiculo.IgnoreQueryFilters()
            .Where(c => CategoriasDemo().Contains(c.Nombre))
            .ToListAsync();
        var clientesDemo = await context.Clientes.IgnoreQueryFilters()
            .Where(c => DniDemo().Contains(c.Dni))
            .ToListAsync();

        if (alquileresDemo.Any())
        {
            context.Alquileres.RemoveRange(alquileresDemo);
        }

        if (vehiculosDemo.Any())
        {
            context.Vehiculos.RemoveRange(vehiculosDemo);
        }

        if (categoriasDemo.Any())
        {
            context.CategoriasVehiculo.RemoveRange(categoriasDemo);
        }

        if (clientesDemo.Any())
        {
            context.Clientes.RemoveRange(clientesDemo);
        }

        await context.SaveChangesAsync();

        await InicializarAsync(servicios);
    }

    private static async Task AsegurarCategoriasAsync(ApplicationDbContext context)
    {
        foreach (var (nombre, precio, deposito) in new (string, decimal, decimal)[]
        {
            ("Compacto", 15000m, 50000m),
            ("SUV", 35000m, 80000m),
            ("Sedán", 22000m, 60000m),
            ("Pickup", 40000m, 90000m),
        })
        {
            if (!await context.CategoriasVehiculo.IgnoreQueryFilters().AnyAsync(c => c.Nombre == nombre))
            {
                context.CategoriasVehiculo.Add(new CategoriaVehiculo
                {
                    Nombre = nombre,
                    Descripcion = $"Categoría {nombre.ToLowerInvariant()} para pruebas del sistema.",
                    PrecioDiario = precio,
                    DepositoGarantia = deposito,
                    Activa = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task AsegurarClientesAsync(ApplicationDbContext context)
    {
        if (await context.Clientes.IgnoreQueryFilters().AnyAsync())
        {
            return;
        }

        var hoy = DateTime.Today;
        var baseClientes = new (string Dni, string Nombre, string Apellido, string Tel, string Email)[]
        {
            ("28999111", "Carlos", "Gómez", "2664112233", "carlos.gomez@email.com"),
            ("35444555", "Ana", "Martínez", "2664556677", "ana.martinez@email.com"),
            ("31222333", "Juan", "Pérez", "2665223344", "juan.perez@email.com"),
            ("30999877", "María", "González", "2665334455", "maria.gonzalez@email.com"),
            ("28555444", "Lucía", "Fernández", "2665445566", "lucia.fernandez@email.com"),
            ("33555666", "Diego", "Rodríguez", "2665556677", "diego.rodriguez@email.com"),
            ("29888777", "Sofía", "López", "2665667788", "sofia.lopez@email.com"),
            ("31999123", "Mateo", "Díaz", "2665778899", "mateo.diaz@email.com"),
            ("30444455", "Valentina", "Torres", "2665889900", "valentina.torres@email.com"),
            ("27777555", "Joaquín", "Méndez", "2665990011", "joaquin.mendez@email.com"),
            ("32666888", "Martina", "Suárez", "2665001122", "martina.suarez@email.com"),
            ("30000111", "Thiago", "Aguirre", "2666112233", "thiago.aguirre@email.com"),
        };

        var licencias = new[] { 2, 1, 3, 2, 4, 1, 2, 5, 2, 3, 1, 4 };
        var nacimientos = new[] { 1982, 1990, 1985, 1992, 1988, 1995, 1991, 1998, 1993, 1986, 1994, 2000 };

        for (var i = 0; i < baseClientes.Length; i++)
        {
            var p = baseClientes[i];
            context.Clientes.Add(new Cliente
            {
                Dni = p.Dni,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                FechaNacimiento = new DateTime(nacimientos[i], (i % 12) + 1, Math.Min(28, (i * 3) % 27 + 1)),
                Telefono = p.Tel,
                Email = p.Email,
                NumeroLicencia = $"LIC-{p.Dni}",
                VencimientoLicencia = hoy.AddYears(licencias[i]),
                Activo = true
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task AsegurarVehiculosAsync(ApplicationDbContext context)
    {
        if (await context.Vehiculos.AnyAsync())
        {
            return;
        }

        var cats = await context.CategoriasVehiculo.IgnoreQueryFilters().ToDictionaryAsync(c => c.Nombre);
        if (!cats.TryGetValue("Compacto", out var compacto)
            || !cats.TryGetValue("SUV", out var suv)
            || !cats.TryGetValue("Sedán", out var sedan)
            || !cats.TryGetValue("Pickup", out var pickup))
        {
            return;
        }

        context.Vehiculos.AddRange(
            new Vehiculo { CategoriaVehiculoId = compacto.Id, Patente = "AF123JK", Marca = "Toyota", Modelo = "Etios", KilometrajeActual = 25000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = compacto.Id, Patente = "AB345CD", Marca = "Chevrolet", Modelo = "Onix", KilometrajeActual = 18000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = compacto.Id, Patente = "AC001EF", Marca = "Renault", Modelo = "Sandero", KilometrajeActual = 31000, EstadoOperativo = EstadoOperativoVehiculo.Mantenimiento },
            new Vehiculo { CategoriaVehiculoId = suv.Id, Patente = "AG987LM", Marca = "Ford", Modelo = "Territory", KilometrajeActual = 12000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = suv.Id, Patente = "AD222GH", Marca = "Volkswagen", Modelo = "Taos", KilometrajeActual = 20000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = suv.Id, Patente = "AE777IJ", Marca = "Jeep", Modelo = "Compass", KilometrajeActual = 28000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = sedan.Id, Patente = "AF555KL", Marca = "Honda", Modelo = "City", KilometrajeActual = 15000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = sedan.Id, Patente = "AG666MN", Marca = "Nissan", Modelo = "Versa", KilometrajeActual = 22000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = sedan.Id, Patente = "AH888OP", Marca = "Toyota", Modelo = "Corolla", KilometrajeActual = 9000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = pickup.Id, Patente = "AI111QR", Marca = "Toyota", Modelo = "Hilux", KilometrajeActual = 35000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = pickup.Id, Patente = "AJ222ST", Marca = "Ford", Modelo = "Ranger", KilometrajeActual = 40000, EstadoOperativo = EstadoOperativoVehiculo.Habilitado },
            new Vehiculo { CategoriaVehiculoId = pickup.Id, Patente = "AK333UV", Marca = "Chevrolet", Modelo = "S10", KilometrajeActual = 27000, EstadoOperativo = EstadoOperativoVehiculo.Baja }
        );

        await context.SaveChangesAsync();
    }

    private static async Task CrearAlquileresAsync(
        ApplicationDbContext context,
        Dictionary<string, CategoriaVehiculo> categorias,
        List<Cliente> clientes,
        List<Vehiculo> vehiculos,
        ApplicationUser admin,
        ApplicationUser empleado)
    {
        if (clientes.Count < 12 || vehiculos.Count < 12)
        {
            return;
        }

        var precioPorCategoria = categorias.ToDictionary(kv => kv.Key, kv => kv.Value.PrecioDiario);
        var vehiculosPorPatente = vehiculos.ToDictionary(v => v.Patente);
        var clientesOrdenados = clientes.OrderBy(c => c.Dni).ToList();

        var hoy = DateTime.Today;
        var alquileres = new List<Alquiler>();

        var reservas = new (string Patente, int ClienteIdx, DateTime Entrega, int Dias)[]
        {
            ("AF123JK", 0, hoy.AddDays(2), 3),
            ("AG987LM", 1, hoy.AddDays(5), 4),
            ("AB345CD", 2, hoy.AddDays(7), 2),
            ("AF555KL", 3, hoy.AddDays(3), 5),
            ("AD222GH", 4, hoy.AddDays(10), 3),
        };
        foreach (var (patente, clienteIdx, entrega, dias) in reservas)
        {
            var vehiculo = vehiculosPorPatente[patente];
            var devolucion = entrega.AddDays(dias);
            var precio = vehiculo.CategoriaVehiculo == null
                ? precioPorCategoria.First().Value
                : vehiculo.CategoriaVehiculo.PrecioDiario;
            alquileres.Add(new Alquiler
            {
                ClienteId = clientesOrdenados[clienteIdx].Id,
                VehiculoId = vehiculo.Id,
                CreadoPorUsuarioId = empleado.Id,
                EntregaPrevista = entrega.Date,
                DevolucionPrevista = devolucion.Date,
                PrecioAcordado = precio,
                ImporteTotal = precio * dias,
                Estado = EstadoAlquiler.Reservado
            });
        }

        var enCurso = new (string Patente, int ClienteIdx, DateTime Entrega, int Dias)[]
        {
            ("AE777IJ", 5, hoy.AddDays(-2), 4),
            ("AG666MN", 6, hoy.AddDays(-4), 6),
            ("AI111QR", 7, hoy.AddDays(-1), 3),
            ("AH888OP", 8, hoy.AddDays(-3), 5),
        };
        foreach (var (patente, clienteIdx, entrega, dias) in enCurso)
        {
            var vehiculo = vehiculosPorPatente[patente];
            var devolucion = entrega.AddDays(dias);
            var precio = vehiculo.CategoriaVehiculo!.PrecioDiario;
            alquileres.Add(new Alquiler
            {
                ClienteId = clientesOrdenados[clienteIdx].Id,
                VehiculoId = vehiculo.Id,
                CreadoPorUsuarioId = empleado.Id,
                EntregaPrevista = entrega.Date,
                DevolucionPrevista = devolucion.Date,
                EntregaReal = entrega.Date.AddHours(10),
                PrecioAcordado = precio,
                ImporteTotal = precio * dias,
                Estado = EstadoAlquiler.EnCurso
            });
        }

        var finalizados = new (string Patente, int ClienteIdx, DateTime Entrega, int Dias, DateTime DevReal, decimal Multa, bool ConDanios)[]
        {
            ("AF123JK", 9, hoy.AddDays(-12), 3, hoy.AddDays(-9), 0m, false),
            ("AG987LM", 10, hoy.AddDays(-20), 4, hoy.AddDays(-16), 0m, false),
            ("AD222GH", 11, hoy.AddDays(-30), 5, hoy.AddDays(-25), 0m, false),
            ("AB345CD", 0, hoy.AddDays(-15), 2, hoy.AddDays(-12), 0m, false),
        };
        foreach (var (patente, clienteIdx, entrega, dias, devReal, multa, conDanios) in finalizados)
        {
            var vehiculo = vehiculosPorPatente[patente];
            var precio = vehiculo.CategoriaVehiculo!.PrecioDiario;
            alquileres.Add(new Alquiler
            {
                ClienteId = clientesOrdenados[clienteIdx].Id,
                VehiculoId = vehiculo.Id,
                CreadoPorUsuarioId = empleado.Id,
                CerradoPorUsuarioId = admin.Id,
                EntregaPrevista = entrega.Date,
                DevolucionPrevista = entrega.AddDays(dias).Date,
                EntregaReal = entrega.Date.AddHours(9),
                DevolucionReal = devReal.Date.AddHours(11),
                PrecioAcordado = precio,
                ImporteTotal = precio * dias,
                Multa = multa,
                Observaciones = "Devolución en fecha prevista: sin recargo por demora.",
                Estado = EstadoAlquiler.Finalizado
            });
        }

        var finDemora = new (string Patente, int ClienteIdx, DateTime Entrega, int Dias, DateTime DevReal, int DiasDemora)[]
        {
            ("AF555KL", 1, hoy.AddDays(-14), 4, hoy.AddDays(-8), 2),
            ("AI111QR", 2, hoy.AddDays(-18), 3, hoy.AddDays(-13), 2),
            ("AJ222ST", 3, hoy.AddDays(-10), 5, hoy.AddDays(-3), 2),
        };
        foreach (var (patente, clienteIdx, entrega, dias, devReal, diasDemora) in finDemora)
        {
            var vehiculo = vehiculosPorPatente[patente];
            var precio = vehiculo.CategoriaVehiculo!.PrecioDiario;
            var demora = diasDemora * precio;
            var danios = 15000m;
            alquileres.Add(new Alquiler
            {
                ClienteId = clientesOrdenados[clienteIdx].Id,
                VehiculoId = vehiculo.Id,
                CreadoPorUsuarioId = empleado.Id,
                CerradoPorUsuarioId = admin.Id,
                EntregaPrevista = entrega.Date,
                DevolucionPrevista = entrega.AddDays(dias).Date,
                EntregaReal = entrega.Date.AddHours(10),
                DevolucionReal = devReal.Date.AddHours(14),
                PrecioAcordado = precio,
                ImporteTotal = precio * dias,
                Multa = demora + danios,
                Observaciones = $"Recargo por demora: {diasDemora} día(s) × {precio:C2} = {demora:C2}. Costos por daños: {danios:C2}.",
                Estado = EstadoAlquiler.Finalizado
            });
        }

        var cancelados = new (string Patente, int ClienteIdx, DateTime Entrega, int Dias)[]
        {
            ("AF123JK", 4, hoy.AddDays(-6), 3),
            ("AG987LM", 5, hoy.AddDays(-4), 4),
            ("AB345CD", 6, hoy.AddDays(-8), 2),
            ("AF555KL", 7, hoy.AddDays(-2), 5),
        };
        foreach (var (patente, clienteIdx, entrega, dias) in cancelados)
        {
            var vehiculo = vehiculosPorPatente[patente];
            var precio = vehiculo.CategoriaVehiculo!.PrecioDiario;
            var importeTotal = precio * dias;
            var multa = importeTotal * 0.25m;
            alquileres.Add(new Alquiler
            {
                ClienteId = clientesOrdenados[clienteIdx].Id,
                VehiculoId = vehiculo.Id,
                CreadoPorUsuarioId = empleado.Id,
                CerradoPorUsuarioId = admin.Id,
                EntregaPrevista = entrega.Date,
                DevolucionPrevista = entrega.AddDays(dias).Date,
                PrecioAcordado = precio,
                ImporteTotal = importeTotal,
                Multa = multa,
                Observaciones = $"Cancelación antes del retiro. Multa por cancelación: 25% del importe original ({importeTotal:C2}) = {multa:C2}.",
                Estado = EstadoAlquiler.Cancelado
            });
        }

        context.Alquileres.AddRange(alquileres);
        await context.SaveChangesAsync();

        var pagos = new List<Pago>();
        decimal AgregarPago(int idxAlquiler, decimal importe, TipoPago tipo, EstadoPago estado, DateTime fecha,
            string medio = "Transferencia", int? anuladoPor = null, DateTime? fechaAnul = null)
        {
            if (idxAlquiler < 0 || idxAlquiler >= alquileres.Count)
            {
                return 0m;
            }

            var alquiler = alquileres[idxAlquiler];
            pagos.Add(new Pago
            {
                AlquilerId = alquiler.Id,
                CreadoPorUsuarioId = empleado.Id,
                Fecha = fecha,
                Importe = decimal.Round(importe, 2),
                Tipo = tipo,
                MedioPago = medio,
                Estado = estado,
                AnuladoPorUsuarioId = anuladoPor,
                FechaAnulacion = fechaAnul
            });
            return importe;
        }

        for (var i = 0; i < alquileres.Count; i++)
        {
            var alquiler = alquileres[i];
            if (alquiler.Estado == EstadoAlquiler.Reservado)
            {
                AgregarPago(i, alquiler.ImporteTotal * 0.20m, TipoPago.Seña, EstadoPago.Vigente,
                    alquiler.EntregaPrevista.AddDays(-1));
            }
            else if (alquiler.Estado == EstadoAlquiler.EnCurso)
            {
                AgregarPago(i, alquiler.ImporteTotal * 0.50m, TipoPago.Parcial, EstadoPago.Vigente,
                    alquiler.EntregaPrevista.AddDays(1));
            }
        }

        foreach (var (idxAlquiler, importe) in Enumerable.Range(0, alquileres.Count)
            .Where(i => alquileres[i].Estado == EstadoAlquiler.Finalizado && alquileres[i].Multa == 0m)
            .SelectMany(i => new[] { (i, alquileres[i].ImporteTotal) }))
        {
            AgregarPago(idxAlquiler, importe, TipoPago.Saldo, EstadoPago.Vigente,
                alquileres[idxAlquiler].DevolucionReal!.Value.AddHours(2), "Efectivo");
        }

        foreach (var i in Enumerable.Range(0, alquileres.Count)
            .Where(i => alquileres[i].Estado == EstadoAlquiler.Finalizado
                && alquileres[i].Multa > 0))
        {
            var alquiler = alquileres[i];
            var fechaPago = alquiler.DevolucionReal!.Value.AddHours(3);

            AgregarPago(
                i,
                alquiler.ImporteTotal,
                TipoPago.Saldo,
                EstadoPago.Vigente,
                fechaPago,
                "Transferencia"
            );

            AgregarPago(
                i,
                alquiler.Multa,
                TipoPago.Recargo,
                EstadoPago.Vigente,
                fechaPago.AddMinutes(5),
                "Transferencia"
            );
        }

        foreach (var i in Enumerable.Range(0, alquileres.Count)
            .Where(i => alquileres[i].Estado == EstadoAlquiler.Cancelado))
        {
            var alquiler = alquileres[i];

            AgregarPago(
                i,
                alquiler.Multa,
                TipoPago.Multa,
                EstadoPago.Vigente,
                alquiler.EntregaPrevista.AddDays(-1),
                "Transferencia"
            );
        }

        pagos.Add(new Pago
        {
            AlquilerId = alquileres[5].Id,
            CreadoPorUsuarioId = empleado.Id,
            Fecha = alquileres[5].EntregaPrevista.AddDays(2),
            Importe = 5000m,
            Tipo = TipoPago.Parcial,
            MedioPago = "Efectivo",
            Estado = EstadoPago.Anulado,
            AnuladoPorUsuarioId = admin.Id,
            FechaAnulacion = alquileres[5].EntregaPrevista.AddDays(2).AddHours(5)
        });

        context.Pagos.AddRange(pagos);
        await context.SaveChangesAsync();

    }

    private static async Task AsegurarPortadasAsync(
        ApplicationDbContext context,
        List<Vehiculo> vehiculos)
    {
        if (!vehiculos.Any())
        {
            return;
        }

        foreach (var vehiculo in vehiculos)
        {
            // Verificar si el vehículo ya tiene una portada.
            var portadaExistente = await context.Imagenes
                .FirstOrDefaultAsync(i =>
                    i.VehiculoId == vehiculo.Id &&
                    i.AlquilerId == null);

            if (portadaExistente != null)
            {
                continue;
            }

            context.Imagenes.Add(new Imagen
            {
                VehiculoId = vehiculo.Id,
                AlquilerId = null,
                Titulo = "Portada",
                Url = $"/uploads/portadas/{vehiculo.Patente}.jpg",
                Descripcion = $"Portada de demo para {vehiculo.Marca} {vehiculo.Modelo} ({vehiculo.Patente})"
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser?> CrearSiNoExisteAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string nombre,
        string apellido,
        string rol)
    {
        var existente = await userManager.FindByEmailAsync(email);
        if (existente != null)
        {
            var rolesActuales = await userManager.GetRolesAsync(existente);
            if (!rolesActuales.Contains(rol))
            {
                await userManager.AddToRoleAsync(existente, rol);
            }

            return existente;
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

        return usuario;
    }
    private static async Task AsegurarAvatarAsync(UserManager<ApplicationUser> userManager, ApplicationUser? usuario, string rutaAvatar)
    {
        if (usuario == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(usuario.Avatar))
        {
            return;
        }

        usuario.Avatar = rutaAvatar;

        var resultado = await userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            var errores = string.Join(
                ", ",
                resultado.Errors.Select(e => e.Description)
            );

            throw new InvalidOperationException(
                $"No se pudo asignar el avatar a {usuario.Email}: {errores}"
            );
        }
    }

    private static string[] PatsDemo() => new[]
    {
        "AF123JK", "AB345CD", "AC001EF",
        "AG987LM", "AD222GH", "AE777IJ",
        "AF555KL", "AG666MN", "AH888OP",
        "AI111QR", "AJ222ST", "AK333UV"
    };

    private static string[] CategoriasDemo() => new[] { "Compacto", "SUV", "Sedán", "Pickup" };

    private static string[] DniDemo() => new[]
    {
        "28999111", "35444555", "31222333", "30999877", "28555444", "33555666",
        "29888777", "31999123", "30444455", "27777555", "32666888", "30000111"
    };
}
