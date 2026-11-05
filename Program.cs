using alquiler_vehiculos_g11.Api;
using alquiler_vehiculos_g11.Data;
using alquiler_vehiculos_g11.Models;
using alquiler_vehiculos_g11.Repositories;
using alquiler_vehiculos_g11.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Reflection;
using System.Text;
using System.Security.Claims;

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
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IImagenRepository, ImagenRepository>();
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

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Falta configurar Jwt:Key.")
                )
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userManager = context.HttpContext.RequestServices
                    .GetRequiredService<UserManager<ApplicationUser>>();

                var identity = context.Principal?.Identity as ClaimsIdentity;

                var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (identity == null || string.IsNullOrEmpty(userId))
                {
                    context.Fail("Token inválido.");
                    return;
                }

                var usuario = await userManager.FindByIdAsync(userId);

                if (usuario == null || !usuario.Activo)
                {
                    context.Fail("Usuario inexistente o inactivo.");
                    return;
                }

                // se actualizan los roles en el token  
                foreach (var claim in identity.FindAll(ClaimTypes.Role).ToList())
                {
                    identity.RemoveClaim(claim);
                }

                var rolesActuales = await userManager.GetRolesAsync(usuario);

                foreach (var rol in rolesActuales)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, rol));
                }
            }
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API - Alquiler de Vehículos",
        Version = "v1",
        Description = "API REST del sistema de alquiler de vehículos."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá tu token JWT."
    });

    options.OperationFilter<SwaggerAuthorizeOperationFilter>();

    options.DocInclusionPredicate((_, apiDesc) =>
        apiDesc.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);

    var archivoXml = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

    if (File.Exists(archivoXml))
    {
        options.IncludeXmlComments(archivoXml);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Alquiler de Vehículos - API";
        options.InjectStylesheet("/css/swagger-dark.css");
    });
}

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
