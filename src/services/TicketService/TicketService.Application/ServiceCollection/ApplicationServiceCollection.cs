using Microsoft.Extensions.DependencyInjection;
using TicketService.Application.Services.Assignment;
using TicketService.Application.Services.Ticket;

namespace TicketService.Application.ServiceDefaults
{
    public static class ApplicationServiceCollection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ITicketCommands, TicketCommands>()
                    .AddScoped<ITicketQueries, TicketQueries>()
                    .AddScoped<IAssignmentCommands, AssignmentCommands>();

        }
    }
}
