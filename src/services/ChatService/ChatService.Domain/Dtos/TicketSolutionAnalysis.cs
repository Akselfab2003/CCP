namespace ChatService.Domain.Dtos
{
    public class TicketSolutionAnalysis
    {
        public string RootCause { get; set; } = string.Empty;
        public string SolutionSummary { get; set; } = string.Empty;
        public string[] SolutionSteps { get; set; } = [];
        public string[] DiagnosticSteps { get; set; } = [];
        public string[] PreventiveTips { get; set; } = [];
    }
}
