using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface IEmailTicketMessageRepository
    {
        Task<Result> AddAsync(EmailTicketMessage emailTicketEntity);
        Task<Result<EmailTicketMessage>> GetByMailIdAsync(string mailId);
        Task<Result<List<EmailTicketMessage>>> GetByTicketIdAsync(int ticketId);
    }
}
