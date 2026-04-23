using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailTicketMessageRepository : IEmailTicketMessageRepository
    {
        private readonly ILogger<EmailTicketMessageRepository> _logger;
        private readonly DBcontext _context;

        public EmailTicketMessageRepository(ILogger<EmailTicketMessageRepository> logger, DBcontext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddAsync(EmailTicketMessage emailTicketEntity)
        {
            try
            {
                await _context.EmailTicketMessages.AddAsync(emailTicketEntity);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding EmailTicketEntity with MailId {MailId}", emailTicketEntity.MessageId);
                return Result.Failure(Error.Failure(code: "EmailTicketEntityAddError", description: $"An error occurred while adding the EmailTicketEntity with MailId {emailTicketEntity.MessageId}."));
            }
        }

        public async Task<Result<EmailTicketMessage>> GetByMailIdAsync(string mailId)
        {
            try
            {
                var entity = await _context.EmailTicketMessages.FirstOrDefaultAsync(e => e.MessageId == mailId);
                if (entity == null)
                {
                    return Result.Failure<EmailTicketMessage>(Error.Failure(code: "EmailTicketEntityNotFound", description: $"No EmailTicketEntity found with MailId {mailId}."));
                }
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EmailTicketEntity with MailId {MailId}", mailId);
                return Result.Failure<EmailTicketMessage>(Error.Failure(code: "EmailTicketEntityRetrievalError", description: $"An error occurred while retrieving the EmailTicketEntity with MailId {mailId}."));
            }
        }

        public async Task<Result<List<EmailTicketMessage>>> GetByTicketIdAsync(int ticketId)
        {
            try
            {
                var entities = await _context.EmailTicketMessages.Where(e => e.TicketId == ticketId).ToListAsync();
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EmailTicketEntities with TicketId {TicketId}", ticketId);
                return Result.Failure<List<EmailTicketMessage>>(Error.Failure(code: "EmailTicketEntitiesRetrievalError", description: $"An error occurred while retrieving the EmailTicketEntities with TicketId {ticketId}."));
            }
        }
    }
}
