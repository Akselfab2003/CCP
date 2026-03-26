using ChatApp.Shared.ResultAbstraction;
using TicketService.Sdk.Models;

namespace TicketService.Sdk.Services.Ticket
{
    internal interface ITicketService
    {
        Task<Result> CreateTicket(CreateTicketRequest request, CancellationToken ct = default);
    }
}