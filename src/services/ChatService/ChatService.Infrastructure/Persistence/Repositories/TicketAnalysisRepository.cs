using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities.AI;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class TicketAnalysisRepository : ITicketAnalysisRepository
    {
        private readonly ILogger<TicketAnalysisRepository> _logger;
        private readonly ChatDbContext _chatDbContext;

        public TicketAnalysisRepository(ILogger<TicketAnalysisRepository> logger, ChatDbContext chatDbContext)
        {
            _logger = logger;
            _chatDbContext = chatDbContext;
        }

        public async Task<Result> AddAsync(TicketAnalysis analysis)
        {
            try
            {
                await _chatDbContext.TicketAnalysis.AddAsync(analysis);
                await _chatDbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving ticket analysis for ticket {TicketId}", analysis.TicketId);
                return Result.Failure(Error.Failure("DatabaseError", "Failed to save ticket analysis."));
            }
        }


        public async Task<Result<TicketAnalysis>> GetByTicketIdAsync(int ticketId)
        {
            try
            {
                var analysis = await _chatDbContext.TicketAnalysis.Include(a => a.Embedding).FirstOrDefaultAsync(a => a.TicketId == ticketId);
                if (analysis == null)
                    return Result.Failure<TicketAnalysis>(Error.NotFound(code: "AnalysisNotFound", description: $"No analysis found for ticket {ticketId}."));
                return Result.Success(analysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket analysis for ticket {TicketId}", ticketId);
                return Result.Failure<TicketAnalysis>(Error.Failure("DatabaseError", "Failed to retrieve ticket analysis."));
            }
        }


        public async Task<Result> UpdateAsync(TicketAnalysis analysis)
        {
            try
            {
                _chatDbContext.TicketAnalysis.Update(analysis);
                await _chatDbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket analysis for ticket {TicketId}", analysis.TicketId);
                return Result.Failure(Error.Failure("DatabaseError", "Failed to update ticket analysis."));
            }
        }

    }
}
