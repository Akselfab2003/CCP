using Microsoft.Extensions.DependencyInjection;
using TicketService.Infrastructure.Persistence.Repositories;
using TicketService.Infrastructure.Persistence.Repositories.Tickets;

namespace TicketService.Infrastructure.ServiceCollection
{
    public static class InfrastructureServiceCollection
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<Domain.Interfaces.ITicketRepositoryCommands, TicketRepositoryCommands>()
                    .AddScoped<Domain.Interfaces.IAssignmentRepository, Persistence.Repositories.AssignmentRepository>()
                    .AddScoped<Domain.Interfaces.ITicketRepositoryQueries, TicketRepositoryQueries>()
                    .AddScoped<Domain.Interfaces.ITicketHistoryRepository, TicketHistoryRepository>();
        }
    }
}
