using ChatService.Domain.Entities.AI;

namespace ChatService.Domain.Dtos
{
    public class SimilarTicket
    {
        public float SimilarityScore { get; set; }
        public required TicketAnalysis TicketAnalysis { get; set; }
    }
}
