namespace ChatService.Domain.Entities.AI
{
    public class TicketAnalysis
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public int TicketId { get; set; }



        public string? ProblemSummary { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? Component { get; set; } = string.Empty;
        public string[] Symptoms { get; set; } = [];
        public string[] ErrorCodes { get; set; } = [];
        public string[] Tags { get; set; } = [];
        public DateTime? ProblemAnalysedAt { get; set; }
        public int ReanalysisCount { get; set; }
        public DateTime? LastReanalysedAt { get; set; }



        public string? RootCause { get; set; }
        public string? SolutionSummary { get; set; }
        public string[]? SolutionSteps { get; set; }
        public string[]? PreventiveTips { get; set; }
        public DateTime? SolutionAnalysedAt { get; set; }


        public TicketEmbedding? Embedding { get; set; }
    }
}
