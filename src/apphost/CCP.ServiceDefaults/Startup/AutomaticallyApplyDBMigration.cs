using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CCP.ServiceDefaults.Startup
{
    public class AutomaticallyApplyDBMigration<T> where T : DbContext
    {
        public static async Task ApplyMigrationsAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            T dbContext = scope.ServiceProvider.GetRequiredService<T>();
            if (await dbContext.Database.CanConnectAsync())
            {
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    await dbContext.Database.MigrateAsync();
                }
            }
        }
    }
}
