using System;
using System.Collections.Generic;
using System.Text;
using ChatApp.Shared.ResultAbstraction;
using TicketService.Sdk.Dtos;

namespace TicketService.Sdk.Services.TicketSdk
{
    public interface ITicketSdkService
    {
        Task<Result<TicketSdkDto>> GetTicketAsync(int ticketId, CancellationToken ct = default);
        Task<Result<List<TicketSdkDto>>> GetTicketsAsync(Guid? assignedUserId = null, Guid? customerId = null, string? status = null, CancellationToken ct = default);
        Task<Result> CreateTicketAsync(string title, Guid? customerId, Guid? assignedUserId, CancellationToken ct = default);
    }
}
