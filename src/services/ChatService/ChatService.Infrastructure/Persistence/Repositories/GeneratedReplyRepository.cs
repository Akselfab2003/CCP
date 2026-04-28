using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities.AI;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class GeneratedReplyRepository : IGeneratedReplyRepository
    {
        private readonly ILogger<GeneratedReplyRepository> _logger;
        private readonly ChatDbContext _context;

        public GeneratedReplyRepository(ILogger<GeneratedReplyRepository> logger, ChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddAsync(GeneratedReply generatedReply)
        {
            try
            {
                await _context.GeneratedReplies.AddAsync(generatedReply);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding GeneratedReply with Id {GeneratedReplyId}", generatedReply.Id);
                return Result.Failure(Error.Failure("DatabaseError", "An error occurred while adding the generated reply to the database."));
            }
        }
    }
}
