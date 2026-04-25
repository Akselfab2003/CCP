using System.Text;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class EmbeddingTextBuilder
    {
        public string BuildProblemText(TicketProblemAnalysis analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Problem: {analysis.Summary}");
            sb.AppendLine($"Category: {analysis.Category}");
            sb.AppendLine($"Component: {analysis.Component}");
            if (analysis.Symptoms.Length > 0)
                sb.AppendLine(
                    $"Symptoms: {string.Join(", ", analysis.Symptoms)}");

            if (analysis.ErrorCodes.Length > 0)
                sb.AppendLine(
                    $"Error codes: {string.Join(", ", analysis.ErrorCodes)}");

            if (analysis.Tags.Length > 0)
                sb.AppendLine(
                    $"Tags: {string.Join(", ", analysis.Tags)}");

            if (analysis.Clarifications.Length > 0)
                sb.AppendLine(
                    $"Additional details: {string.Join(", ", analysis.Clarifications)}");

            return sb.ToString().Trim();
        }
        public string BuildSolutionText(
       TicketAnalysis storedAnalysis,
       TicketSolutionAnalysis solution)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Problem: {storedAnalysis.ProblemSummary}");
            sb.AppendLine($"Category: {storedAnalysis.Category}");
            sb.AppendLine($"Component: {storedAnalysis.Component}");

            if (storedAnalysis.Symptoms?.Length > 0)
                sb.AppendLine(
                    $"Symptoms: {string.Join(", ", storedAnalysis.Symptoms)}");

            if (storedAnalysis.ErrorCodes?.Length > 0)
                sb.AppendLine(
                    $"Error codes: {string.Join(", ", storedAnalysis.ErrorCodes)}");

            if (storedAnalysis.Tags?.Length > 0)
                sb.AppendLine(
                    $"Tags: {string.Join(", ", storedAnalysis.Tags)}");

            sb.AppendLine($"Root cause: {solution.RootCause}");
            sb.AppendLine($"Solution: {solution.SolutionSummary}");

            if (solution.SolutionSteps.Length > 0)
                sb.AppendLine(
                    $"Steps: {string.Join(", ", solution.SolutionSteps)}");

            if (solution.PreventionTips.Length > 0)
                sb.AppendLine(
                    $"Prevention: {string.Join(", ", solution.PreventionTips)}");

            return sb.ToString().Trim();
        }

    }
}
