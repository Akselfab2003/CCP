using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CCP.ServiceDefaults.swagger
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAnonymouse = context.MethodInfo.GetCustomAttributes(true)
                                                  .OfType<AllowAnonymousAttribute>()
                                                  .Any();

            if (hasAnonymouse)
                return;


            var hasAuthorize = (context.MethodInfo.GetCustomAttributes(true)
                                     .OfType<AuthorizeAttribute>()
                                     .Any() || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false));


            if (!hasAuthorize)
                return;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"), new List<string>()
                        }
                    }
            };
        }
    }
}
