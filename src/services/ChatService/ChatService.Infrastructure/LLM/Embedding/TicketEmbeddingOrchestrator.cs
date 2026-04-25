using ChatService.Domain.Dtos;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.Persistence;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class TicketEmbeddingOrchestrator
    {
        private readonly ILogger<TicketEmbeddingOrchestrator> _logger;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly ChatDbContext _chatDbContext;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public TicketEmbeddingOrchestrator(ILogger<TicketEmbeddingOrchestrator> logger,
                                           ITicketAnalysisService ticketAnalysisService,
                                           ChatDbContext chatDbContext,
                                           [FromKeyedServices("qwen")] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _logger = logger;
            _ticketAnalysisService = ticketAnalysisService;
            _chatDbContext = chatDbContext;
            _embeddingGenerator = embeddingGenerator;
        }

        public async Task OnTicketCreatedAsync(SupportTicket ticket)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for ticket {TicketId}", ticket.TicketId);
            }
        }

        public async Task OnNewMessageAsync(SupportTicket ticket)
        {
        }

        public async Task OnTicketClosedAsync(SupportTicket ticket)
        {
        }
    }
}
