using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace CCP.ServiceDefaults.swagger
{
    public static class SwaggerAppMapper
    {
        public static void AppMapSwaggerExtensions(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "v1");
                options.ShowExtensions();
                options.ShowCommonExtensions();
                options.DisplayRequestDuration();
                if (app.Environment.IsDevelopment())
                {
                    options.EnablePersistAuthorization();
                }
            });
        }
    }
}
