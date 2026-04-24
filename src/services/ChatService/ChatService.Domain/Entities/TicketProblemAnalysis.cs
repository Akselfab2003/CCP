namespace ChatService.Infrastructure.LLM.Analysis
{
    public class TicketProblemAnalysis
    {
        public string Summary { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;

        public string[] Symptoms { get; set; } = [];
        public string[] ErrorCodes { get; set; } = [];
        public string[] Tags { get; set; } = [];

    }
}
