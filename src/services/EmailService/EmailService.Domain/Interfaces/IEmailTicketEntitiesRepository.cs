using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface IEmailTicketEntitiesRepository
    {
        Task<Result> AddAsync(EmailTicketEntities emailTicketEntity);
        Task<Result<EmailTicketEntities>> GetByMailIdAsync(string mailId);
        Task<Result<List<EmailTicketEntities>>> GetByTicketIdAsync(int ticketId);
    }
}
