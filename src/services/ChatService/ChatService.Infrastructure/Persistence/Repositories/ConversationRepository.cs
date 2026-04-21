using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ILogger<ConversationRepository> _logger;
        private readonly ChatDbContext _context;

        public ConversationRepository(ILogger<ConversationRepository> logger, ChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddConversation(ConversationEntity conversation)
        {
            try
            {
                await _context.Conversations.AddAsync(conversation);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding conversation with ID {ConversationId}", conversation.Id);
                return Result.Failure(Error.Failure(code: "AddConversationError", description: $"An error occurred while adding the conversation: {ex.Message}"));
            }
        }

        public async Task<Result<ConversationEntity>> GetConversationById(Guid conversationId)
        {
            try
            {
                var conversation = await _context.Conversations.Where(c => c.Id == conversationId).SingleOrDefaultAsync();
                if (conversation == null)
                    return Result.Failure<ConversationEntity>(Error.Failure(code: "ConversationNotFound", description: $"No conversation found with ID {conversationId}"));

                return Result.Success(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation with ID {ConversationId}", conversationId);
                return Result.Failure<ConversationEntity>(Error.Failure(code: "GetConversationError", description: $"An error occurred while retrieving the conversation: {ex.Message}"));
            }
        }


        public async Task<Result<List<ConversationEntity>>> GetConversationsBySessionId(Guid SessionId)
        {
            try
            {
                var conversations = await _context.Conversations.Where(c => c.SessionId == SessionId).ToListAsync();
                if (conversations == null || !conversations.Any())
                    return Result.Failure<List<ConversationEntity>>(Error.Failure(code: "ConversationNotFound", description: $"No conversation found with Session ID {SessionId}"));
                return Result.Success(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving conversation with Session ID {SessionId}", SessionId);
                return Result.Failure<List<ConversationEntity>>(Error.Failure(code: "GetConversationsBySessionIdError", description: $"An error occurred while retrieving the conversation: {ex.Message}"));
            }

        }
    }
}
