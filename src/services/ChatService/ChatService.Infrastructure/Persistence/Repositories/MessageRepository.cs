using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ILogger<MessageRepository> _logger;
        private readonly ChatDbContext _context;

        public MessageRepository(ILogger<MessageRepository> logger, ChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddMessage(MessageEntity message)
        {
            try
            {
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding message with ID {MessageId}", message.Id);
                return Result.Failure(Error.Failure(code: "AddMessageError", description: $"An error occurred while adding the message: {ex.Message}"));
            }
        }

        public async Task<Result<List<MessageEntity>>> GetMessagesByConversationId(Guid conversationId)
        {
            try
            {
                var messages = await _context.Messages.Where(m => m.ConversationId == conversationId).ToListAsync();
                return Result.Success(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving messages for Conversation ID {ConversationId}", conversationId);
                return Result.Failure<List<MessageEntity>>(Error.Failure(code: "GetMessagesError", description: $"An error occurred while retrieving the messages: {ex.Message}"));
            }
        }
    }
}
