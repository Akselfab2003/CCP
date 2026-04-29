using Pgvector;

namespace ChatService.Domain.Entities.AI
{
    public class TicketEmbedding
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public int TicketId { get; set; }
        public Guid AnalysisId { get; set; }

        public string ProblemEmbeddingSource { get; set; } = string.Empty;

        public Vector ProblemVector { get; set; } = null!;
        public DateTime ProblemEmbeddedAt { get; set; }


        public string? SolutionEmbeddingSource { get; set; }
        public Vector? SolutionVector { get; set; }
        public DateTime? SolutionEmbeddedAt { get; set; }


        public bool IsSemanticSearchable { get; set; }
        public TicketAnalysis Analysis { get; set; } = null!;
    }
}
