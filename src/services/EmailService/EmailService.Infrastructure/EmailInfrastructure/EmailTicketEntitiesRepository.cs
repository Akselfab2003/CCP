using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailTicketEntitiesRepository : IEmailTicketEntitiesRepository
    {
        private readonly ILogger<EmailTicketEntitiesRepository> _logger;
        private readonly DBcontext _context;

        public EmailTicketEntitiesRepository(ILogger<EmailTicketEntitiesRepository> logger, DBcontext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddAsync(EmailTicketEntities emailTicketEntity)
        {
            try
            {
                await _context.EmailTicketLookup.AddAsync(emailTicketEntity);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding EmailTicketEntity with MailId {MailId}", emailTicketEntity.MailId);
                return Result.Failure(Error.Failure(code: "EmailTicketEntityAddError", description: $"An error occurred while adding the EmailTicketEntity with MailId {emailTicketEntity.MailId}."));
            }
        }

        public async Task<Result<EmailTicketEntities>> GetByMailIdAsync(string mailId)
        {
            try
            {
                var entity = await _context.EmailTicketLookup.FirstOrDefaultAsync(e => e.MailId == mailId);
                if (entity == null)
                {
                    return Result.Failure<EmailTicketEntities>(Error.Failure(code: "EmailTicketEntityNotFound", description: $"No EmailTicketEntity found with MailId {mailId}."));
                }
                return Result.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EmailTicketEntity with MailId {MailId}", mailId);
                return Result.Failure<EmailTicketEntities>(Error.Failure(code: "EmailTicketEntityRetrievalError", description: $"An error occurred while retrieving the EmailTicketEntity with MailId {mailId}."));
            }
        }

        public async Task<Result<List<EmailTicketEntities>>> GetByTicketIdAsync(int ticketId)
        {
            try
            {
                var entities = await _context.EmailTicketLookup.Where(e => e.TicketId == ticketId).ToListAsync();
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving EmailTicketEntities with TicketId {TicketId}", ticketId);
                return Result.Failure<List<EmailTicketEntities>>(Error.Failure(code: "EmailTicketEntitiesRetrievalError", description: $"An error occurred while retrieving the EmailTicketEntities with TicketId {ticketId}."));
            }
        }
    }
}
