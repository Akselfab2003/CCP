using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CCP.ServiceDefaults.swagger
{
    public static class OpenApiConfiguration
    {
        public static void SetupOpenApiForSwagger(this OpenApiOptions options)
        {
            options.AddDocumentTransformer((document, request, ct) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Description = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Scheme = "bearer",
                });

                document.Security = [
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"), new List<string>()
                        }
                    }
                ];
                document.RegisterComponents();
                document.SetReferenceHostDocument();
                return Task.CompletedTask;
            });
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                // Fix union types: ["integer", "string"] → "integer"
                if (schema.Type.HasValue)
                {
                    var type = schema.Type.Value;

                    if (type.HasFlag(JsonSchemaType.Integer) && type.HasFlag(JsonSchemaType.String))
                    {
                        schema.Type = type & ~JsonSchemaType.String;
                        schema.Pattern = null;
                    }

                    if (type.HasFlag(JsonSchemaType.Number) && type.HasFlag(JsonSchemaType.String))
                    {
                        schema.Type = type & ~JsonSchemaType.String;
                        schema.Pattern = null;
                    }
                }

                return Task.CompletedTask;
            });

        }
    }
}
