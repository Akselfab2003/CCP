using ChatService.Domain.Dtos;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public interface ITicketEmbeddingOrchestrator
    {
        Task OnNewMessageAsync(SupportTicket ticket);
        Task OnTicketClosedAsync(SupportTicket ticket);
        Task OnTicketCreatedAsync(SupportTicket ticket);
    }
}