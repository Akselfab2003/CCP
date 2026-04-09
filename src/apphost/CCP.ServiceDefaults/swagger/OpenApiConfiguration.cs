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
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Fix nullable $ref: oneOf: [null, $ref] → OpenApiSchemaReference
                // .NET OpenAPI v2 emits nullable object properties as oneOf:[{type:null},{$ref:T}]
                // Kiota can't handle this — it generates broken union wrapper types.
                // We replace the oneOf entry in each schema's Properties with a direct OpenApiSchemaReference.
                if (document.Components?.Schemas is null)
                    return Task.CompletedTask;

                foreach (var (_, schema) in document.Components.Schemas)
                {
                    if (schema is not OpenApiSchema openApiSchema || openApiSchema.Properties is null)
                        continue;

                    var propertiesToFix = new Dictionary<string, string>();

                    foreach (var (propName, propSchema) in openApiSchema.Properties)
                    {
                        if (propSchema is not OpenApiSchema ps || ps.OneOf is not { Count: 2 })
                            continue;

                        var nullVariant = ps.OneOf.FirstOrDefault(s => s is OpenApiSchema { Type: JsonSchemaType.Null });
                        var refVariant = ps.OneOf.OfType<OpenApiSchemaReference>().FirstOrDefault();

                        if (nullVariant != null && refVariant?.Reference?.Id is string refId)
                            propertiesToFix[propName] = refId;
                    }

                    foreach (var (propName, refId) in propertiesToFix)
                        openApiSchema.Properties[propName] = new OpenApiSchemaReference(refId);
                }

                return Task.CompletedTask;
            });

        }
    }
}
