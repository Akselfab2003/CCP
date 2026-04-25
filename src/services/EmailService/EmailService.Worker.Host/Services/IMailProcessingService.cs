using CCP.Shared.ResultAbstraction;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public interface IMailProcessingService
    {
        Task<Result<(int TicketId, Guid CustomerId)>> ProcessIncomingMailAsync(MimeMessage message);
    }
}