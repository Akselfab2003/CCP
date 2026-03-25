using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CCP.ServiceDefaults.swagger
{
    public class RoleOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true)
                                                        .Union(context.MethodInfo.GetCustomAttributes(true))
                                                        .OfType<AuthorizeAttribute>()
                                                        .Union(context.MethodInfo.DeclaringType!.GetCustomAttributes(true)
                                                                                                .OfType<AuthorizeAttribute>());

            var roles = authorizeAttributes.Where(a => !string.IsNullOrEmpty(a.Roles))
                                           .SelectMany(a => a.Roles!.Split(',').Select(r => r.Trim()))
                                           .Distinct()
                                           .ToList();

            if (roles.Any())
            {
                operation.Description += $"<p><strong>Required Roles:</strong> {string.Join(", ", roles)}</p>";
            }
        }
    }
}
