using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var mensajes = options.ModelBindingMessageProvider;

    mensajes.SetMissingBindRequiredValueAccessor(
        campo => $"Falta el valor para {campo}.");
    mensajes.SetMissingKeyOrValueAccessor(
        () => "Falta un valor requerido.");
    mensajes.SetMissingRequestBodyRequiredValueAccessor(
        () => "El cuerpo de la solicitud no puede estar vacío.");
    mensajes.SetValueMustNotBeNullAccessor(
        _ => "El valor ingresado no es válido.");
    mensajes.SetAttemptedValueIsInvalidAccessor(
        (valor, campo) => $"El valor '{valor}' no es válido para {campo}.");
    mensajes.SetNonPropertyAttemptedValueIsInvalidAccessor(
        valor => $"El valor '{valor}' no es válido.");
    mensajes.SetUnknownValueIsInvalidAccessor(
        campo => $"El valor proporcionado no es válido para {campo}.");
    mensajes.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => "El valor proporcionado no es válido.");
    mensajes.SetValueIsInvalidAccessor(
        valor => $"El valor '{valor}' no es válido.");
    mensajes.SetValueMustBeANumberAccessor(
        campo => $"{campo} debe ser un número.");
    mensajes.SetNonPropertyValueMustBeANumberAccessor(
        () => "El valor debe ser un número.");
});
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICategoriaVehiculoRepository, CategoriaVehiculoRepository>();
builder.Services.AddScoped<IAlquilerRepository, AlquilerRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IAlquilerService, AlquilerService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();
builder.Services.AddScoped<IInformeClientesRepository, ClienteInformeRepository>();



var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection' en appsettings.json.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(cadenaConexion, ServerVersion.AutoDetect(cadenaConexion)));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Acceso/Login";
    options.LogoutPath = "/Acceso/Logout";
    options.AccessDeniedPath = "/Home/Error";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await DatosIniciales.InicializarAsync(scope.ServiceProvider);
}

app.Run();
