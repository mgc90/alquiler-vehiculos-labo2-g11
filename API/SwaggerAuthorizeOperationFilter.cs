using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace alquiler_vehiculos_g11.Api;

/// <summary>
/// Agrega el requisito de seguridad Bearer únicamente a las operaciones protegidas.
/// </summary>
public class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var atributos = context.MethodInfo.GetCustomAttributes(inherit: true)
            .Concat(
                context.MethodInfo.DeclaringType?.GetCustomAttributes(inherit: true)
                ?? Array.Empty<object>())
            .ToArray();

        var requiereAutorizacion = atributos.OfType<AuthorizeAttribute>().Any();
        var permiteAnonimo = atributos.OfType<AllowAnonymousAttribute>().Any();

        if (!requiereAutorizacion || permiteAnonimo)
        {
            return;
        }

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = new List<string>()
            }
        };
    }
}
