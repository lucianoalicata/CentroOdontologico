using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Clinica.Api.Documentacion;

public sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer
{
    private readonly IAuthenticationSchemeProvider _esquemas;

    public BearerSecurityTransformer(IAuthenticationSchemeProvider esquemas) => _esquemas = esquemas;

    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var registrados = await _esquemas.GetAllSchemesAsync();
        if (!registrados.Any(e => e.Name == JwtBearerDefaults.AuthenticationScheme))
            return;

        document.Components ??= new OpenApiComponents();

        // En OpenAPI v2 se instancia directamente OpenApiSecurityScheme
        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Ingresá tu token JWT en el formato: Bearer {token}"
        };

        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = securityScheme;

        // Requerimiento de seguridad global para las operaciones
        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
        };

        if (document.Paths is not null)
        {
            foreach (var path in document.Paths.Values)
            {
                if (path.Operations is null) continue;

                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(securityRequirement);
                }
            }
        }
    }
}